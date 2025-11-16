using BLL.Interface;
using BLL.Model.Request.Exam;
using BLL.Model.Response;
using BLL.Model.Response.Exam;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SWD_Grading.Controllers
{
	[Route("api/exams")]
	[ApiController]
	public class ExamController : ControllerBase
	{

		private readonly IExamService _examService;
		public ExamController(IExamService examService)
		{
			_examService = examService;
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
				Message =  "Get exam successfully",
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
				Message =  "Update exam successfully",
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
	}
}
