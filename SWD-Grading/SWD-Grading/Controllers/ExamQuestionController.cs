using BLL.Interface;
using BLL.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SWD_Grading.Controllers
{
	[Route("api/exam-questions")]
	[ApiController]
	public class ExamQuestionController : ControllerBase
	{
		private readonly ITesseractOcrService _ocrService;

		public ExamQuestionController(TesseractOcrService ocrService)
		{
			_ocrService = ocrService;
		}

		[HttpPost("image")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> ExtractText([FromForm] IFormFile file)
		{
			if (file == null || file.Length == 0)
				return BadRequest("No file uploaded.");

			// tạo file tạm
			var tempFilePath = Path.GetTempFileName();
			using (var stream = new FileStream(tempFilePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			// OCR (default ENG)
			string text;
			try
			{
				text = _ocrService.ExtractText(tempFilePath, "eng");
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"OCR error: {ex.Message}");
			}

			// xóa file tạm
			System.IO.File.Delete(tempFilePath);

			return Ok(new
			{
				code = 200,
				message = "OCR completed successfully",
				text = text
			});
		}
	}
}
