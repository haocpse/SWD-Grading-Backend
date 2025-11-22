using BLL.Interface;
using BLL.Model.Response;
using Microsoft.AspNetCore.Authorization;
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
	[Route("api")]
	[Authorize]
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
	/// Upload ZIP file containing Student_Solutions folder
	/// </summary>
	/// <param name="examId">Exam ID</param>
	/// <param name="file">ZIP file containing Student_Solutions folder with all student submissions</param>
	/// <returns>Upload response with ExamZip ID</returns>
	[HttpPost("exams/{examId}/upload-zip")]
	[Consumes("multipart/form-data")]
	[RequestSizeLimit(524288000)] // 500 MB
	[RequestFormLimits(MultipartBodyLengthLimit = 524288000)] // 500 MB
	[ProducesResponseType(typeof(UploadStudentSolutionsResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> UploadStudentSolutions(
		[FromRoute] long examId,
		IFormFile file)
		{
			try
			{
				if (file == null || file.Length == 0)
				{
					return BadRequest(new UploadStudentSolutionsResponse
					{
						ExamZipId = 0,
						Status = "Error",
						Message = "No file uploaded"
					});
				}

				_logger.LogInformation($"Initiating upload for Exam ID: {examId}, File: {file.FileName} ({file.Length} bytes)");

				var examZipId = await _examUploadService.InitiateUploadAsync(file, examId);

				_logger.LogInformation($"Upload initiated successfully. ExamZip ID: {examZipId}");

				return Ok(new UploadStudentSolutionsResponse
				{
					ExamZipId = examZipId,
					Status = "Processing",
					Message = $"File '{file.FileName}' uploaded successfully and processing has started. Check status using the provided ExamZipId."
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
	/// <param name="id">ExamZip ID</param>
	/// <returns>Processing status response</returns>
	[HttpGet("exam-zips/{id}/check-status")]
	[ProducesResponseType(typeof(ProcessingStatusResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> GetProcessingStatus([FromRoute] long id)
	{
		try
		{
			_logger.LogInformation($"Fetching processing status for ExamZip ID: {id}");

			var status = await _examUploadService.GetProcessingStatusAsync(id);

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

	/// <summary>
	/// Get all exam zips with paging and filters
	/// </summary>
	/// <param name="filter">Filter parameters (page, size, examId, status)</param>
	/// <returns>Paged list of exam zips</returns>
	[HttpGet("exam-zips")]
	[ProducesResponseType(typeof(BaseResponse<PagingResponse<ExamZipResponse>>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> GetAllExamZips([FromQuery] ExamZipFilter filter)
	{
		try
		{
			var result = await _examUploadService.GetAllExamZipsAsync(filter);

			BaseResponse<PagingResponse<ExamZipResponse>> response = new()
			{
				Code = 200,
				Message = "Get exam zips successfully",
				Data = result
			};

			return Ok(response);
		}
		catch (ArgumentException ex)
		{
			_logger.LogWarning($"Validation error: {ex.Message}");
			return BadRequest(new
			{
				Code = 400,
				Message = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching exam zips");
			return StatusCode(500, new
			{
				Code = 500,
				Message = $"Internal server error: {ex.Message}"
			});
		}
	}
	}
}

