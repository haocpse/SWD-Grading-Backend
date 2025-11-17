using BLL.Interface;
using BLL.Model.Response;
using DAL.Interface;
using Model.Enums;
using Model.Request;
using Model.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
	public class ExamStudentService : IExamStudentService
	{
		private readonly IUnitOfWork _unitOfWork;

		public ExamStudentService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<PagingResponse<ExamStudentResponse>> GetExamStudentsByExamIdAsync(long examId, ExamStudentFilter filter)
		{
			// Validate pagination
			if (filter.Page <= 0)
				throw new ArgumentException("Page must be greater than 0");
			if (filter.Size <= 0)
				throw new ArgumentException("Size must be greater than 0");

			// Parse status filter
			ExamStudentStatus? statusFilter = null;
			if (!string.IsNullOrEmpty(filter.Status))
			{
				if (Enum.TryParse<ExamStudentStatus>(filter.Status, true, out var status))
				{
					statusFilter = status;
				}
				else
				{
					throw new ArgumentException($"Invalid status: {filter.Status}. Valid values: NOT_FOUND, PARSED, GRADED");
				}
			}

			var skip = (filter.Page - 1) * filter.Size;

			// Get total count
			var totalItems = await _unitOfWork.ExamStudentRepository
				.CountByExamIdAsync(examId, statusFilter);

			// Get paged ExamStudents
			var examStudents = await _unitOfWork.ExamStudentRepository
				.GetByExamIdWithDetailsAsync(examId, skip, filter.Size, statusFilter);

			if (!examStudents.Any())
			{
				return new PagingResponse<ExamStudentResponse>
				{
					Result = new List<ExamStudentResponse>(),
					Page = filter.Page,
					Size = filter.Size,
					TotalItems = 0,
					TotalPages = 0
				};
			}

			// Get all DocFiles for these ExamStudents
			var examStudentIds = examStudents.Select(es => es.Id).ToList();
			var docFiles = await _unitOfWork.DocFileRepository
				.GetByExamStudentIdsAsync(examStudentIds);

			// Group DocFiles by ExamStudentId
			var docFilesByExamStudent = docFiles
				.GroupBy(df => df.ExamStudentId)
				.ToDictionary(g => g.Key, g => g.ToList());

			// Map to response
			var result = examStudents.Select(es => new ExamStudentResponse
			{
				ExamStudentId = es.Id,
				StudentCode = es.Student.StudentCode,
				StudentName = es.Student.FullName,
				Status = es.Status.ToString(),
				Note = es.Note,
				DocFiles = docFilesByExamStudent.ContainsKey(es.Id)
					? docFilesByExamStudent[es.Id].Select(df => new DocFileResponse
					{
						DocFileId = df.Id,
						FileName = df.FileName,
						FilePath = df.FilePath,
						ParseStatus = df.ParseStatus.ToString()
					}).ToList()
					: new List<DocFileResponse>()
			}).ToList();

			return new PagingResponse<ExamStudentResponse>
			{
				Result = result,
				Page = filter.Page,
				Size = filter.Size,
				TotalItems = totalItems,
				TotalPages = (int)Math.Ceiling((double)totalItems / filter.Size)
			};
		}
	}
}



