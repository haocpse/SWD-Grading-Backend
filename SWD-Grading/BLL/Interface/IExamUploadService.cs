using BLL.Model.Response;
using Microsoft.AspNetCore.Http;
using Model.Request;
using Model.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
	public interface IExamUploadService
	{
		/// <summary>
		/// Initiate upload of ZIP file containing Student_Solutions folder
		/// </summary>
		/// <param name="zipFile">ZIP file containing Student_Solutions folder</param>
		/// <param name="examId">Exam ID</param>
		/// <returns>ExamZip ID</returns>
		Task<long> InitiateUploadAsync(IFormFile zipFile, long examId);

		/// <summary>
		/// Get processing status of an exam ZIP upload
		/// </summary>
		/// <param name="examZipId">ExamZip ID</param>
		/// <returns>Processing status response</returns>
		Task<ProcessingStatusResponse> GetProcessingStatusAsync(long examZipId);

		/// <summary>
		/// Get all exam zips with paging and filters
		/// </summary>
		/// <param name="filter">Filter parameters</param>
		/// <returns>Paged exam zip response</returns>
		Task<PagingResponse<ExamZipResponse>> GetAllExamZipsAsync(ExamZipFilter filter);
	}
}

