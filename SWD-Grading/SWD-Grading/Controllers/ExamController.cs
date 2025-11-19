using BLL.Interface;
using BLL.Model.Request.Exam;
using BLL.Model.Response;
using BLL.Model.Response.Exam;
using BLL.Model.Response.Grade;
using BLL.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Request;
using Model.Response;
using SWD_Grading.Helper;

namespace SWD_Grading.Controllers
{
	[Route("api/exams")]
	[ApiController]
	public class ExamController : ControllerBase
	{

		private readonly IExamService _examService;
		private readonly IExamStudentService _examStudentService;
		private readonly ITesseractOcrService _ocrService;
		public ExamController(IExamService examService, ITesseractOcrService ocrService, IExamStudentService examStudentService)
		{
			_examService = examService;
			_ocrService = ocrService;
			_examStudentService = examStudentService;
		}

		[HttpPost]
		public async Task<IActionResult> CreateExam([FromBody] CreateExamRequest request)
		{
			BaseResponse<ExamResponse> response = new()
			{
				Code = 201,
				Message = "Create exam successfully",
				Data = await _examService.CreateExam(request)
			};
			return StatusCode(201, response);
		}
		[HttpGet]
		public async Task<IActionResult> GetAllExams([FromQuery] ExamFilter filter)
		{
			var result = await _examService.GetAllAsync(filter);

			BaseResponse<PagingResponse<ExamResponse>> response = new()
			{
				Code = 200,
				Message = "Get all exams successfully",
				Data = result
			};

			return Ok(response);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetExamById(long id)
		{
			var result = await _examService.GetByIdAsync(id);

			BaseResponse<ExamResponse?> response = new()
			{
				Code = 200,
				Message = "Get exam successfully",
				Data = result
			};

			return Ok(response);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateExam(long id, [FromBody] UpdateExamRequest request)
		{
			var result = await _examService.UpdateAsync(id, request);

			BaseResponse<ExamResponse?> response = new()
			{
				Code = 200,
				Message = "Update exam successfully",
				Data = result
			};

			return Ok(response);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteExam(long id)
		{
			var success = await _examService.DeleteAsync(id);

			BaseResponse<bool> response = new()
			{
				Code = 204,
				Message = "Delete exam successfully",
				Data = success
			};

			return NoContent();
		}


		[HttpPut("{id}/description")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> ExtractText([FromRoute] long id, IFormFile file)
		{
			if (file == null || file.Length == 0)
				return BadRequest("No file uploaded.");

			var tempFilePath = Path.GetTempFileName();

			try
			{
				using (var stream = new FileStream(tempFilePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				string url = await _ocrService.ExtractText(id, tempFilePath, file, "eng");

				return Ok(new
				{
					code = 200,
					message = "OCR completed successfully",
					problemStatement = url
				});
			}
			catch (Exception ex)
			{
				// Log lỗi
				Console.WriteLine("OCR ERROR: " + ex);

				return StatusCode(500, new
				{
					code = 500,
					message = "Internal Server Error",
					detail = ex.Message
				});
			}
			finally
			{
				System.IO.File.Delete(tempFilePath);
			}
		}

		[HttpPost("{id}/details")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> ParseDetailExcel([FromRoute] long id, IFormFile file)
		{
			if (file == null || file.Length == 0)
				return BadRequest("No file uploaded.");

			await _examService.ParseDetailExcel(id, file);

			return Ok(new
			{
				message = "Import exam details successfully."
			});
		}

		[HttpGet("{examId}/students")]
		public async Task<IActionResult> GetExamStudents(long examId, [FromQuery] ExamStudentFilter filter)
		{
			var result = await _examStudentService.GetExamStudentsByExamIdAsync(examId, filter);

			BaseResponse<PagingResponse<ExamStudentResponse>> response = new()
			{
				Code = 200,
				Message = "Get exam students successfully",
				Data = result
			};

			return Ok(response);
		}

		[HttpGet("{id}/questions")]
		public async Task<IActionResult> GetQuestionsByExamId([FromRoute] long id)
		{
			var result = await _examService.GetQuestionByExamId(id);
			BaseResponse<ExamResponse> response = new()
			{
				Code = 200,
				Message = "Get exam questions successfully",
				Data = result
			};

			return Ok(response);
		}

		[HttpPost("{id}/export-excel")]
		public async Task<IActionResult> ExportGradeExcel([FromRoute] long id)
		{
			int userId = User.GetUserId();
			var result = await _examService.ExportGradeExcel(userId, id);
			BaseResponse<GradeExportResponse> response = new()
			{
				Code = 200,
				Message = "Get exam questions successfully",
				Data = result
			};

			return Ok(response);
		}

		[HttpPost("{id}/grade-excel")]
		public async Task<IActionResult> GradeExcelHistory([FromRoute] long id)
		{
			var result = await _examService.GetGradeHistory(id);
			BaseResponse<List<GradeExportResponse>> response = new()
			{
				Code = 200,
				Message = "Get exam questions successfully",
				Data = result
			};

			return Ok(response);
		}
	}
}
