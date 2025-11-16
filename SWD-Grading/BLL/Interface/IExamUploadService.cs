using Microsoft.AspNetCore.Http;
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
		/// Initiate upload of student solutions ZIP file
		/// </summary>
		/// <param name="zipFile">ZIP file containing student solutions</param>
		/// <param name="examId">Exam ID</param>
		/// <param name="examCode">Exam code for S3 path structure</param>
		/// <returns>ExamZip ID</returns>
		Task<long> InitiateUploadAsync(IFormFile zipFile, long examId, string examCode);

		/// <summary>
		/// Get processing status of an exam ZIP upload
		/// </summary>
		/// <param name="examZipId">ExamZip ID</param>
		/// <returns>Processing status response</returns>
		Task<ProcessingStatusResponse> GetProcessingStatusAsync(long examZipId);
	}
}

