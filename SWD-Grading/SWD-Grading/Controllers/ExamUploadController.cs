using BLL.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Request;
using Model.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD_Grading.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ExamUploadController : ControllerBase
	{
		private readonly IExamUploadService _examUploadService;
		private readonly ILogger<ExamUploadController> _logger;

		public ExamUploadController(IExamUploadService examUploadService, ILogger<ExamUploadController> logger)
		{
			_examUploadService = examUploadService;
			_logger = logger;
		}

		/// <summary>
		/// Upload student solutions ZIP file
		/// </summary>
		/// <param name="examId">Exam ID</param>
		/// <param name="file">ZIP file containing student solutions</param>
		/// <returns>Upload response with ExamZip ID</returns>
		[HttpPost("upload-solutions/{examId}")]
		[ProducesResponseType(typeof(UploadStudentSolutionsResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> UploadStudentSolutions(
			[FromRoute] long examId,
			[FromForm] IFormFile file,
			[FromForm] string examCode)
		{
			try
			{
				if (file == null)
				{
					return BadRequest(new UploadStudentSolutionsResponse
					{
						ExamZipId = 0,
						Status = "Error",
						Message = "No file uploaded"
					});
				}

				if (string.IsNullOrEmpty(examCode))
				{
					return BadRequest(new UploadStudentSolutionsResponse
					{
						ExamZipId = 0,
						Status = "Error",
						Message = "ExamCode is required"
					});
				}

				_logger.LogInformation($"Initiating upload for Exam ID: {examId}, ExamCode: {examCode}");

				var examZipId = await _examUploadService.InitiateUploadAsync(file, examId, examCode);

				_logger.LogInformation($"Upload initiated successfully. ExamZip ID: {examZipId}");

				return Ok(new UploadStudentSolutionsResponse
				{
					ExamZipId = examZipId,
					Status = "Processing",
					Message = "File uploaded successfully and processing has started. Check status using the provided ExamZipId."
				});
			}
			catch (ArgumentException ex)
			{
				_logger.LogWarning($"Validation error: {ex.Message}");
				return BadRequest(new UploadStudentSolutionsResponse
				{
					ExamZipId = 0,
					Status = "Error",
					Message = ex.Message
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while uploading student solutions");
				return StatusCode(500, new UploadStudentSolutionsResponse
				{
					ExamZipId = 0,
					Status = "Error",
					Message = $"Internal server error: {ex.Message}"
				});
			}
		}

		/// <summary>
		/// Get processing status of uploaded exam ZIP
		/// </summary>
		/// <param name="examZipId">ExamZip ID</param>
		/// <returns>Processing status response</returns>
		[HttpGet("status/{examZipId}")]
		[ProducesResponseType(typeof(ProcessingStatusResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetProcessingStatus([FromRoute] long examZipId)
		{
			try
			{
				_logger.LogInformation($"Fetching processing status for ExamZip ID: {examZipId}");

				var status = await _examUploadService.GetProcessingStatusAsync(examZipId);

				return Ok(status);
			}
			catch (ArgumentException ex)
			{
				_logger.LogWarning($"ExamZip not found: {ex.Message}");
				return NotFound(new
				{
					Status = "Error",
					Message = ex.Message
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while fetching processing status");
				return StatusCode(500, new
				{
					Status = "Error",
					Message = $"Internal server error: {ex.Message}"
				});
			}
		}
	}
}

