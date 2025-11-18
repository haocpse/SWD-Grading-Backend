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
using BLL.Model.Response.Grade;
using Amazon;
using Amazon.S3;
using Model.Configuration;
using Microsoft.Extensions.Configuration;
using Amazon.S3.Model;
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;

namespace BLL.Service
{
	public class ExamService : IExamService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IGradeService _gradeService;
		private readonly IS3Service _s3Service;
		private readonly IAmazonS3 _s3Client;
		private readonly AwsConfiguration _awsConfig;
		public ExamService(IUnitOfWork unitOfWork, IMapper mapper, IGradeService gradeService, IS3Service s3Service, IAmazonS3 s3Client, IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_gradeService = gradeService;
			_s3Service = s3Service;
			_s3Client = s3Client;
			_awsConfig = new AwsConfiguration();
			configuration.GetSection("AWS").Bind(_awsConfig);
		}

		public async Task<ExamResponse> CreateExam(CreateExamRequest request)
		{
			bool isDuplicatedCode = await _unitOfWork.ExamRepository.GetByExamCodeAsync(request.ExamCode) == null ? false : true;
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
			// 0. Lấy exam để có examCode
			var exam = await _unitOfWork.ExamRepository.GetByIdAsync(examId);
			if (exam == null)
			{
				throw new Exception("Exam not found");
			}
			var examCode = exam.ExamCode; // sửa theo property thực tế
										  // 1. Copy file vào MemoryStream
			using var ms = new MemoryStream();
			await file.CopyToAsync(ms);

			// 2. Đặt Position về 0 để đọc Excel
			ms.Position = 0;
			using var doc = SpreadsheetDocument.Open(ms, false);

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
			ms.Position = 0; // RẤT QUAN TRỌNG: reset về đầu trước khi upload

			var s3Path = $"{examCode}/original-file"; // examCode/original-file
			var originalFileUrl = await _s3Service.UploadExcelFileAsync(ms, file.FileName, s3Path);

			// Nếu muốn lưu URL lại trong bảng Exam:
			exam.OriginalExcel = originalFileUrl;

			// (exam đang được tracking bởi DbContext, chỉ cần gán là đủ)
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

		public async Task<GradeExportResponse> ExportGradeExcel(int userId, long id)
		{
			// 1. Load file template từ S3
			var exam = await _unitOfWork.ExamRepository.GetByIdAsync(id);
			if (exam == null)
				throw new AppException("Exam not found", 404);

			var key = GetS3KeyFromUrl(exam.OriginalExcel);
			var original = await DownloadFromS3Async(key);

			// Copy stream để chỉnh sửa bằng OpenXML
			var ms = new MemoryStream();
			original.CopyTo(ms);
			ms.Position = 0;

			var settings = new OpenSettings { AutoSave = true };

			using (var doc = SpreadsheetDocument.Open(ms, true, settings))
			{
				var wbPart = doc.WorkbookPart!;
				var sheet = wbPart.Workbook.Sheets
					.Cast<Sheet>()
					.First(s => s.Name!.Value.Contains("Marking"));

				var wsPart = (WorksheetPart)wbPart.GetPartById(sheet.Id!);
				var ws = wsPart.Worksheet;
				var rows = ws.GetFirstChild<SheetData>()!.Elements<Row>().ToList();

				//---------------------------------------------------------
				// 3. Mapping rubric row
				//---------------------------------------------------------
				int colD = 3, colL = 11;
				Row rubricRow = rows[1];
				var rubricCells = rubricRow.Elements<Cell>().ToList();
				var rubricMap = new Dictionary<string, int>();

				for (int col = colD; col <= colL; col++)
				{
					var name = GetCellValue(doc, rubricCells[col]);
					if (!string.IsNullOrWhiteSpace(name))
						rubricMap[name.Trim()] = col;
				}

				//---------------------------------------------------------
				// 4. Load student scores
				//---------------------------------------------------------
				var examStudents = await _unitOfWork.ExamStudentRepository.GetExamStudentByExamId(userId, id);
				int rowStart = 3;

				var currentTeacherCode = examStudents[0].Teacher.TeacherCode;
				HideOtherTeacherRows(doc, wsPart, currentTeacherCode);

				rows = ws.GetFirstChild<SheetData>()!.Elements<Row>().ToList();
				//---------------------------------------------------------
				// 5. Xóa merge cells (nếu tồn tại)
				//---------------------------------------------------------

				//---------------------------------------------------------
				// 6. Fill scores — GIỮ CÔNG THỨC
				//---------------------------------------------------------
				for (int i = 0; i < examStudents.Count; i++)
				{
					var stud = examStudents[i];
					var grade = stud.Grades
						.Where(g => g.Status == GradeStatus.GRADED)
						.OrderByDescending(g => g.Attempt)
						.FirstOrDefault();
					if (grade == null) continue;

					var row = FindRowByStudentCode(doc, wsPart, rows, stud.Student.StudentCode);
					if (row == null) continue;

					foreach (var detail in grade.Details)
					{
						string cri = detail.Rubric.Criterion.Trim();
						decimal score = detail.Score;
						if (!rubricMap.TryGetValue(cri, out int col)) continue;

						var cell = GetOrCreateCell(wsPart, row, col);

						// ❗ Nếu ô có công thức → không ghi đè
						if (cell.CellFormula != null)
						{
							Console.WriteLine($"[DEBUG] Skip formula cell: {cell.CellReference}");
							continue;
						}

						// Ghi giá trị
						cell.CellValue = new CellValue(score.ToString());
						cell.DataType = CellValues.Number;
					}
				}
				// ❗ BẢO TOÀN CÔNG THỨC → KHÔNG XOÁ calcChain
				// KHÔNG ĐỤNG TỚI calcChain.xml
				var calcProps = wbPart.Workbook.CalculationProperties;

				if (calcProps == null)
				{
					calcProps = new CalculationProperties()
					{
						CalculationId = 0,
						ForceFullCalculation = true,
						FullCalculationOnLoad = true
					};
					wbPart.Workbook.Append(calcProps);
				}
				else
				{
					calcProps.ForceFullCalculation = true;
					calcProps.FullCalculationOnLoad = true;
				}

				ws.Save();
				wbPart.Workbook.Save();
			}

			//---------------------------------------------------------
			// 7. Upload file lên S3
			//---------------------------------------------------------
			ms.Position = 0;
			string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			string fileName = $"GradeExport_[{exam.ExamCode}]_[{timestamp}].xlsx";
			string uploadPath = $"{exam.ExamCode}/grade-export";
			string url = await _s3Service.UploadExcelFileAsync(ms, fileName, uploadPath);

			//---------------------------------------------------------
			// 8. Lưu DB
			//---------------------------------------------------------
			var export = new GradeExport
			{
				ExamId = id,
				UserId = userId,
				Url = url,
				CreatedAt = DateTime.UtcNow
			};

			await _unitOfWork.GradeExportRepository.AddAsync(export);
			await _unitOfWork.SaveChangesAsync();

			return new GradeExportResponse { Url = url };
		}

		private Row? FindRowByStudentCode(SpreadsheetDocument doc, WorksheetPart wsPart, List<Row> rows, string studentCode)
		{
			int studentColIndex = 1; // B column

			foreach (var row in rows)
			{
				var cell = GetOrCreateCell(wsPart, row, studentColIndex);
				string value = GetCellValue(doc, cell);

				if (!string.IsNullOrWhiteSpace(value) &&
					value.Trim().Equals(studentCode.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					return row;
				}
			}

			return null;
		}

		private void HideOtherTeacherRows(SpreadsheetDocument doc, WorksheetPart wsPart, string teacherCode)
		{
			var sheetData = wsPart.Worksheet.GetFirstChild<SheetData>();
			var rows = sheetData.Elements<Row>().ToList();

			int markerColIndex = 2; // Column C

			foreach (var row in rows.Where(r => r.RowIndex >= 3))
			{
				var markerCell = GetOrCreateCell(wsPart, row, markerColIndex);
				string markerValue = GetCellValue(doc, markerCell)?.Trim() ?? "";

				if (!markerValue.Equals(teacherCode.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					row.Hidden = true;    // ⭐ Chỉ ẩn, không xóa
				}
			}
		}

		private Cell GetOrCreateCell(WorksheetPart wsPart, Row row, int colIndex)
		{
			string columnName = GetColumnName(colIndex);
			string cellReference = columnName + row.RowIndex;

			Cell? cell = row.Elements<Cell>()
						   .FirstOrDefault(c => c.CellReference?.Value == cellReference);

			if (cell == null)
			{
				cell = new Cell { CellReference = cellReference };

				Cell? refCell = null;
				foreach (Cell c in row.Elements<Cell>())
				{
					if (string.Compare(c.CellReference.Value, cellReference, true) > 0)
					{
						refCell = c;
						break;
					}
				}

				row.InsertBefore(cell, refCell);
			}

			return cell;
		}

		private string GetColumnName(int index)
		{
			int dividend = index + 1;
			string columnName = "";

			while (dividend > 0)
			{
				int modulo = (dividend - 1) % 26;
				columnName = Convert.ToChar(65 + modulo) + columnName;
				dividend = (dividend - modulo) / 26;
			}

			return columnName;
		}


		private string GetS3KeyFromUrl(string url)
		{
			var uri = new Uri(url);

			// AbsolutePath => trả về decode nhưng dấu + vẫn giữ nguyên
			var path = Uri.UnescapeDataString(uri.AbsolutePath);

			// Trong S3, folder/file name chứa space phải là " " không phải "+"
			path = path.Replace("+", " ");

			return path.TrimStart('/');
		}

		private async Task<MemoryStream> DownloadFromS3Async(string key)
		{
			var request = new GetObjectRequest
			{
				BucketName = _awsConfig.BucketName,
				Key = key
			};

			var response = await _s3Client.GetObjectAsync(request);

			var ms = new MemoryStream();
			await response.ResponseStream.CopyToAsync(ms);
			ms.Position = 0;

			return ms;
		}

		private MemoryStream CloneStream(Stream original)
		{
			var clone = new MemoryStream();
			original.Position = 0;
			original.CopyTo(clone);
			clone.Position = 0;
			original.Position = 0;
			return clone;
		}

		public MemoryStream ConvertXmlExcelToXlsx(Stream xmlFile)
		{
			// Set license for EPPlus 8.x
			ExcelPackage.License.SetNonCommercialPersonal("MyProject");

			using var package = new ExcelPackage(xmlFile);

			var ms = new MemoryStream();
			package.SaveAs(ms);
			ms.Position = 0;

			return ms;
		}

		public async Task<PagingResponse<ExamResponse>> GetAssignedExam(ExamFilter filter, int userId)
		{
			if (filter.Page <= 0)
				throw new AppException("Page number must be greater than or equal to 1", 400);

			if (filter.Size < 0)
				throw new AppException("Size must not be negative", 400);

			var filters = new List<Expression<Func<Exam, bool>>>();

			// Teacher được gán grading exam
			filters.Add(e => e.ExamStudents.Any(es => es.TeacherId == userId));

			var skip = (filter.Page - 1) * filter.Size;

			// Sort newest first
			Func<IQueryable<Exam>, IOrderedQueryable<Exam>> orderBy =
				q => q.OrderByDescending(o => o.CreatedAt);
			var totalItems = await _unitOfWork.ExamRepository.CountAsync(filters);

			var data = await _unitOfWork.ExamRepository.GetPagedAsync<Exam>(
				skip,
				filter.Size,
				filters,
				orderBy,
				include: q => q.Include(x => x.ExamStudents),
				null,
				asNoTracking: true
			);
			var respones = _mapper.Map<IEnumerable<ExamResponse>>(data.ToList());
			return new PagingResponse<ExamResponse>
			{
				Result = respones,
				Page = filter.Page,
				Size = filter.Size,
				TotalItems = totalItems,
				TotalPages = (int)Math.Ceiling(totalItems / (double)filter.Size)
			};
		}
	}
}
