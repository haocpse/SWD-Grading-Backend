using BLL.Interface;
using BLL.Model.Request.ExamQuestion;
using BLL.Model.Request.Rubric;
using BLL.Model.Response;
using BLL.Model.Response.ExamQuestion;
using BLL.Model.Response.Rubric;
using BLL.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SWD_Grading.Controllers
{
	[Route("api/exam-questions")]
	[ApiController]
	public class ExamQuestionController : ControllerBase
	{

		private readonly IExamQuestionService _examQuestionService;
		private readonly IRubricService _rubricService;

		public ExamQuestionController(IExamQuestionService examQuestionService, IRubricService rubricService)
		{
			_examQuestionService = examQuestionService;
			_rubricService = rubricService;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(long id)
		{
			var item = await _examQuestionService.GetByIdAsync(id);

			return Ok(new BaseResponse<ExamQuestionResponse?>
			{
				Code = 200,
				Message = "Get exam question successfully",
				Data = item
			});
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(long id, [FromBody] UpdateExamQuestionRequest req)
		{
			var item = await _examQuestionService.UpdateAsync(id, req);
			return Ok(new BaseResponse<ExamQuestionResponse?>
			{
				Code = 200,
				Message = "Update exam question successfully",
				Data = item
			});
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(long id)
		{
			await _examQuestionService.DeleteAsync(id);
			
			return NoContent();
		}

		[HttpGet("{id}/rubrics")]
		public async Task<IActionResult> GetRubricsByQuestionId([FromRoute] long id)
		{
			var items = await _rubricService.GetRubricByQuestionId(id);
			return Ok(new BaseResponse<IEnumerable<RubricResponse>>
			{
				Code = 200,
				Message = "Get rubrics successfully",
				Data = items
			});
		}

		[HttpPost("{id}/rubrics")]
		public async Task<IActionResult> AddRubricForQuestion([FromRoute] long id, [FromBody] CreateRubricRequest request)
		{
			var item = await _rubricService.CreateAsync(id, request);
			return Ok(new BaseResponse<RubricResponse>
			{
				Code = 200,
				Message = "Add rubrics successfully",
				Data = item
			});
		}
	}
}
