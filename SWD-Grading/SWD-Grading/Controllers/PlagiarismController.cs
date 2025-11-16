using BLL.Interface;
using BLL.Model.Request;
using BLL.Model.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SWD_Grading.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PlagiarismController : ControllerBase
	{
		private readonly IPlagiarismService _plagiarismService;

		public PlagiarismController(IPlagiarismService plagiarismService)
		{
			_plagiarismService = plagiarismService;
		}

	/// <summary>
	/// Check plagiarism for all submissions in an exam
	/// </summary>
	/// <param name="examId">The exam ID to check</param>
	/// <param name="request">Plagiarism check parameters including threshold</param>
	/// <returns>Plagiarism check results with suspicious pairs</returns>
	[HttpPost("check/{examId}")]
	public async Task<ActionResult<BaseResponse<PlagiarismCheckResponse>>> CheckPlagiarism(
		long examId,
		[FromBody] CheckPlagiarismRequest request)
	{
		try
		{
			// Use default userId = 1 (system check) if not authenticated
			int userId = 1;
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
			if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int authenticatedUserId))
			{
				userId = authenticatedUserId;
			}

			var result = await _plagiarismService.CheckPlagiarismAsync(examId, request.Threshold, userId);

			return Ok(new BaseResponse<PlagiarismCheckResponse>
			{
				Success = true,
				Message = $"Plagiarism check completed. Found {result.SuspiciousPairsCount} suspicious pair(s).",
				Data = result
			});
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new BaseResponse<object>
			{
				Success = false,
				Message = ex.Message
			});
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(new BaseResponse<object>
			{
				Success = false,
				Message = ex.Message
			});
		}
		catch (Exception ex)
		{
			return StatusCode(500, new BaseResponse<object>
			{
				Success = false,
				Message = $"Internal server error: {ex.Message}"
			});
		}
	}

	/// <summary>
	/// Get plagiarism check history for an exam
	/// </summary>
	/// <param name="examId">The exam ID</param>
	/// <returns>List of previous plagiarism checks</returns>
	[HttpGet("history/{examId}")]
	public async Task<ActionResult<BaseResponse<object>>> GetCheckHistory(long examId)
	{
		try
		{
			var history = await _plagiarismService.GetCheckHistoryAsync(examId);

			return Ok(new BaseResponse<object>
			{
				Success = true,
				Message = $"Retrieved {history.Count} plagiarism check(s)",
				Data = history
			});
		}
		catch (Exception ex)
		{
			return StatusCode(500, new BaseResponse<object>
			{
				Success = false,
				Message = $"Internal server error: {ex.Message}"
			});
		}
	}

	/// <summary>
	/// Manually trigger embedding generation for a specific document
	/// </summary>
	/// <param name="docFileId">The document file ID</param>
	/// <returns>Success message</returns>
	[HttpPost("generate-embedding/{docFileId}")]
	public async Task<ActionResult<BaseResponse<object>>> GenerateEmbedding(long docFileId)
	{
		try
		{
			await _plagiarismService.GenerateEmbeddingForDocFileAsync(docFileId);

			return Ok(new BaseResponse<object>
			{
				Success = true,
				Message = "Embedding generated successfully"
			});
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new BaseResponse<object>
			{
				Success = false,
				Message = ex.Message
			});
		}
		catch (Exception ex)
		{
			return StatusCode(500, new BaseResponse<object>
			{
				Success = false,
				Message = $"Internal server error: {ex.Message}"
			});
		}
	}
	}
}

