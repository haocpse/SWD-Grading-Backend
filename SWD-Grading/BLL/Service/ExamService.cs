using AutoMapper;
using BLL.Exceptions;
using BLL.Interface;
using BLL.Model.Request.Exam;
using BLL.Model.Response;
using BLL.Model.Response.Exam;
using DAL.Interface;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Http;
using Model.Entity;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using BLL.Model.Request.Student;
using Model.Enums;
using BLL.Model.Response.ExamQuestion;
using BLL.Model.Response.Rubric;
using Amazon.Runtime.Telemetry.Tracing;
using Grpc.Core;
using BLL.Model.Request.Grade;

namespace BLL.Service
{
	public class ExamService : IExamService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IGradeService _gradeService;
		public ExamService(IUnitOfWork unitOfWork, IMapper mapper, IGradeService gradeService)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_gradeService = gradeService;
		}

		public async Task<ExamResponse> CreateExam(CreateExamRequest request)
		{		
			bool isDuplicatedCode =  await _unitOfWork.ExamRepository.GetByExamCodeAsync(request.ExamCode) == null ? false : true;
			if (isDuplicatedCode)
				throw new AppException("Duplicated exam code", 400);
			Exam exam = _mapper.Map<Exam>(request);
			await _unitOfWork.ExamRepository.AddAsync(exam);
			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<ExamResponse>(exam);
		}

		public async Task<bool> DeleteAsync(long id)
		{
			var exam = await _unitOfWork.ExamRepository.GetByIdAsync(id);
			if (exam == null)
				throw new AppException("Exam not found", 404);
			await _unitOfWork.ExamRepository.RemoveAsync(exam);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		public async Task<PagingResponse<ExamResponse>> GetAllAsync(ExamFilter filter)
		{
			var filters = new List<Expression<Func<Exam, bool>>>();
			if (filter.Page <= 0)
				throw new AppException("Page number must be greater than or equal to 1", 400);
			if (filter.Size < 0)
				throw new AppException("Size must not be negative", 400);
			var skip = (filter.Page - 1) * filter.Size;
			var totalItems = await _unitOfWork.ExamRepository.CountAsync(filters);
			Func<IQueryable<Exam>, IOrderedQueryable<Exam>> orderBy = q => q.OrderByDescending(o => o.CreatedAt);
			var data = await _unitOfWork.ExamRepository.GetPagedAsync<Exam>(
				skip, filter.Size, filters, orderBy, null, null, asNoTracking: true);
			var respones = _mapper.Map<IEnumerable<ExamResponse>>(data.ToList());
			return new()
			{
				Result = respones,
				Page = filter.Page,
				Size = filter.Size,
				TotalItems = totalItems,
				TotalPages = (int)Math.Ceiling((double)totalItems / filter.Size)
			};
		}

		public async Task<ExamResponse?> GetByIdAsync(long id)
		{
			var exam = await _unitOfWork.ExamRepository.GetByIdAsync(id);
			return exam == null ?
				throw new AppException("Exam not found", 404) :
				_mapper.Map<ExamResponse>(exam);
		}

		public async Task ParseDetailExcel(long examId, IFormFile file)
		{
			using var stream = file.OpenReadStream();
			using var doc = SpreadsheetDocument.Open(stream, false);

			var wb = doc.WorkbookPart!;
			var sheet = wb.Workbook.Sheets!.GetFirstChild<Sheet>()!;
			var wsPart = (WorksheetPart)wb.GetPartById(sheet.Id!);
			var sheetData = wsPart.Worksheet.GetFirstChild<SheetData>()!;
			var rows = sheetData.Elements<Row>().ToList();

			//--------------------------------------------------------------------
			// 1) READ PART NAME + DESCRIPTIONS → ExamQuestion + Rubric
			//--------------------------------------------------------------------
			Row partRow = rows[0];   // Part 1 / Part 2 / Part 3...
			Row descRow = rows[1];   // Description
			Row scoreRow = rows[2];  // MaxScore của từng rubric

			var partCells = partRow.Elements<Cell>().ToList();
			var descCells = descRow.Elements<Cell>().ToList();
			var scoreCells = scoreRow.Elements<Cell>().ToList();

			// Bỏ 2 cột cuối Total + Comment
			int colCount = partCells.Count - 2;

			List<ExamQuestion> questions = new();
			List<Rubric> rubrics = new();

			int questionNumber = 1;
			ExamQuestion? currentQuestion = null;
			decimal currentPartTotal = 0;

			for (int c = 3; c < colCount; c++)
			{
				string partName = GetCellValue(doc, partCells[c]);
				string desc = GetCellValue(doc, descCells[c]);
				string scoreStr = GetCellValue(doc, scoreCells[c]);

				decimal rubricMaxScore = 0;
				decimal.TryParse(scoreStr, out rubricMaxScore);
				bool isDuplicatedQuestion = false;
				// Nếu gặp "Part 1", "Part 2", "Part 3"
				if (!string.IsNullOrWhiteSpace(partName))
				{
					isDuplicatedQuestion = await _unitOfWork.ExamQuestionRepository.ExistQuestionByExamIdAndQuestionName(examId, partName);
					if (!isDuplicatedQuestion)
					{
						// Nếu đang ở part trước → cập nhật tổng điểm
						if (currentQuestion != null)
						{
							currentQuestion.MaxScore = currentPartTotal;
						}

						// Reset
						currentPartTotal = 0;

						currentQuestion = new ExamQuestion
						{
							ExamId = examId,
							QuestionNumber = questionNumber++,
							QuestionText = partName,
							MaxScore = 0 // sẽ cập nhật sau
						};

						questions.Add(currentQuestion);
					}
				}

				// Thêm Rubric thuộc part hiện tại
				if (!isDuplicatedQuestion && currentQuestion != null && !string.IsNullOrWhiteSpace(desc))
				{
					rubrics.Add(new Rubric
					{
						ExamQuestion = currentQuestion,
						Criterion = desc,
						MaxScore = rubricMaxScore,
						OrderIndex = rubrics.Count + 1
					});

					currentPartTotal += rubricMaxScore; // cộng dồn điểm cho Part
				}
			}

			// Sau vòng lặp, cập nhật Part cuối cùng
			if (currentQuestion != null)
			{
				currentQuestion.MaxScore = currentPartTotal;
			}

			//--------------------------------------------------------------------
			// 2) READ STUDENT + EXAMSTUDENT
			//--------------------------------------------------------------------
			List<Student> students = new();
			List<ExamStudent> examStudents = new();

			for (int r = 3; r < rows.Count; r++)
			{
				var cells = rows[r].Elements<Cell>().ToList();
				if (cells.Count < 3) continue;

				string solution = GetCellValue(doc, cells[1]); // StudentCode
				string markerCode = GetCellValue(doc, cells[2]); // TeacherCode

				if (string.IsNullOrWhiteSpace(solution))
					continue;

				//--------------------------------------------------------------
				// Create Student
				//--------------------------------------------------------------
				(bool existed, Student student) result = await GetOrCreateStudentAsync(solution);
				if (!result.existed)
					students.Add(result.student);

				//--------------------------------------------------------------
				// Get or create Teacher
				//--------------------------------------------------------------
				var teacher = await GetOrCreateTeacherAsync(markerCode);

				//--------------------------------------------------------------
				// Add ExamStudent
				//--------------------------------------------------------------
				(bool existed, ExamStudent? es) resultExamStudent = await GetOrCreateExamStudentAsync(examId, result.student, result.existed);
				if (!resultExamStudent.existed)
				{
					examStudents.Add(new ExamStudent
					{
						ExamId = examId,
						Student = result.student,
						TeacherId = teacher.Id,
						Status = ExamStudentStatus.NOT_FOUND, // default
						Note = null
					});
				}

			}

			//--------------------------------------------------------------------
			// 3) SAVE ALL → chỉ SaveChanges 1 lần cho hiệu suất
			//--------------------------------------------------------------------
			bool saved = false;
			if (students.Count > 0)
			{
				await _unitOfWork.StudentRepository.AddRangeAsync(students);
				saved = true;
			}
				
			if (examStudents.Count > 0)
			{
				await _unitOfWork.ExamStudentRepository.AddRangeAsync(examStudents);
				saved = true;
			}
			if (questions.Count > 0)
			{
				await _unitOfWork.ExamQuestionRepository.AddRangeAsync(questions);
				saved = true;
			}
				
			if (rubrics.Count > 0)
			{
				await _unitOfWork.RubricRepository.AddRangeAsync(rubrics);
				saved = true;
			}	

			if (saved)
			{
				await _unitOfWork.SaveChangesAsync();
				if (examStudents.Count > 0)
					await CreateGradeForExamStudent(examId, examStudents);
			}
				
		}

		private async Task<User> GetOrCreateTeacherAsync(string teacherCode)
		{
			// Try get existing
			var teacher = await _unitOfWork.UserRepository.GetByTeacherCodeAsync(teacherCode);
			if (teacher != null)
				return teacher;

			// Create new
			teacher = new User
			{
				Username = teacherCode,
				TeacherCode = teacherCode,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
				IsActive = true,
				Role = UserRole.TEACHER
			};

			await _unitOfWork.UserRepository.AddAsync(teacher);

			// VERY IMPORTANT: SAVE so EF generates Teacher.Id
			await _unitOfWork.SaveChangesAsync();

			return teacher;
		}

		private async Task<(bool existed, Student s)> GetOrCreateStudentAsync(string solution)
		{
			// Try get existing
			var student = await _unitOfWork.StudentRepository.GetByStudentCodeAsync(solution);
			if (student != null)
				return (true, student);

			// Create new
			student = new Student
			{
				StudentCode = solution,
				FullName = solution,
				Email = $"{solution}@fpt.edu.vn"
			};

			return (false, student);
		}

		private async Task<(bool existed, ExamStudent? es)> GetOrCreateExamStudentAsync(long examId, Student student, bool isExsitedStudent)
		{
			// Try get existing
			if (isExsitedStudent)
			{
				var examStudent = await _unitOfWork.ExamStudentRepository.GetByExamAndStudentAsync(examId, student.Id);
				if (examStudent != null)
					return (true, examStudent);
			}
			return (false, null);
		}

		private string GetCellValue(SpreadsheetDocument doc, Cell cell)
		{
			if (cell.CellValue == null)
				return "";

			string value = cell.CellValue.InnerText;

			if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
			{
				var table = doc.WorkbookPart!.SharedStringTablePart!.SharedStringTable;
				return table.ChildElements[int.Parse(value)].InnerText;
			}

			return value;
		}

		public async Task<ExamResponse?> UpdateAsync(long id, UpdateExamRequest request)
		{
			var exam = await _unitOfWork.ExamRepository.GetByIdAsync(id);
			if (exam == null)
				throw new AppException("Exam not found", 404);
			_mapper.Map(request, exam);
			await _unitOfWork.ExamRepository.UpdateAsync(exam);
			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<ExamResponse>(exam);
		}

		public async Task<ExamResponse?> GetQuestionByExamId(long id)
		{
			var exam = await _unitOfWork.ExamRepository.GetByIdAsync(id);
			if (exam == null)
				throw new AppException("Exam not found", 404);
			ExamResponse response = _mapper.Map<ExamResponse>(exam);
			IEnumerable<ExamQuestion> questions = await _unitOfWork.ExamQuestionRepository.GetQuestionByExamId(id);
			List<ExamQuestionResponse> questionResponses = _mapper.Map<List<ExamQuestionResponse>>(questions.ToList());
			foreach (var question in questionResponses)
			{
				IEnumerable<Rubric> rubrics = await _unitOfWork.RubricRepository.GetRubricByQuestionId(question.Id);
				question.Rubrics = _mapper.Map<List<RubricResponse>>(rubrics.ToList());
			}
			response.Questions = questionResponses;
			return response;
		}

		private async Task CreateGradeForExamStudent(long examId, List<ExamStudent> students)
		{
			var requests = new List<AddGradeRangeRequest>();
			foreach (var student in students)
			{
				requests.Add(new AddGradeRangeRequest
				{
					ExamStudentId = student.Id, 
					TotalScore = 0,        
					Comment = "",
					GradedAt = DateTime.UtcNow,
					GradedBy = null,
					Attempt = 1,
					Status = GradeStatus.CREATED
				});
			}

			await _gradeService.CreateRange(examId, requests);
		}

	}
}
