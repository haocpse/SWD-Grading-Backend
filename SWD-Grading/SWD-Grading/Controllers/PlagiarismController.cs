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
		/// Check plagiarism for a suspicious document against all other documents in the same exam
	/// </summary>
		/// <param name="docFileId">The suspicious document file ID to check</param>
	/// <param name="request">Plagiarism check parameters including threshold</param>
		/// <returns>Plagiarism check results with similar documents found</returns>
		[HttpPost("check-document/{docFileId}")]
		public async Task<ActionResult<BaseResponse<PlagiarismCheckResponse>>> CheckSuspiciousDocument(
			long docFileId,
		[FromBody] CheckPlagiarismRequest request)
	{
		try
		{
			// Use default userId = 1 (system check) if not authenticated
			int userId = 2;
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
			if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int authenticatedUserId))
			{
				userId = authenticatedUserId;
			}

				var result = await _plagiarismService.CheckSuspiciousDocumentAsync(docFileId, request.Threshold, userId);

			return Ok(new BaseResponse<PlagiarismCheckResponse>
			{
				Success = true,
					Message = $"Plagiarism check completed for document {docFileId}. Found {result.SuspiciousPairsCount} similar document(s).",
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

	/// <summary>
	/// Verify a similarity result using AI (GPT) to confirm if documents are truly similar
	/// </summary>
	/// <param name="similarityResultId">The similarity result ID to verify</param>
	/// <returns>Verification result with AI analysis</returns>
	[HttpPost("verify-with-ai/{similarityResultId}")]
	public async Task<ActionResult<BaseResponse<VerificationResponse>>> VerifyWithAI(long similarityResultId)
	{
		try
		{
			var result = await _plagiarismService.VerifyWithAIAsync(similarityResultId);

			return Ok(new BaseResponse<VerificationResponse>
			{
				Success = true,
				Message = $"AI verification completed. Result: {(result.AIVerifiedSimilar == true ? "Similar" : "Not Similar")}",
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
	/// Teacher manually verifies a similarity result
	/// </summary>
	/// <param name="similarityResultId">The similarity result ID to verify</param>
	/// <param name="request">Verification details (is_similar, notes)</param>
	/// <returns>Verification result</returns>
	[HttpPost("teacher-verify/{similarityResultId}")]
	public async Task<ActionResult<BaseResponse<VerificationResponse>>> TeacherVerify(
		long similarityResultId,
		[FromBody] TeacherVerifyRequest request)
	{
		try
		{
			// Use default userId = 1 (system) if not authenticated
			int userId = 1;
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
			if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int authenticatedUserId))
			{
				userId = authenticatedUserId;
			}

			var result = await _plagiarismService.TeacherVerifyAsync(
				similarityResultId, 
				request.IsSimilar, 
				request.Notes, 
				userId);

			return Ok(new BaseResponse<VerificationResponse>
			{
				Success = true,
				Message = $"Teacher verification completed. Result: {(request.IsSimilar ? "Similar" : "Not Similar")}",
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
