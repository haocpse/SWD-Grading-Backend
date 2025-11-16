using BLL.Interface;
using BLL.Model.Request.ExamQuestion;
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

		public ExamQuestionController(IExamQuestionService examQuestionService)
		{
			_examQuestionService = examQuestionService;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(long id)
		{
			var item = await _examQuestionService.GetByIdAsync(id);
			if (item == null) return NotFound();
			return Ok(item);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(long id, [FromBody] UpdateExamQuestionRequest req)
		{
			var item = await _examQuestionService.UpdateAsync(id, req);
			if (item == null) return NotFound();
			return Ok(item);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(long id)
		{
			var ok = await _examQuestionService.DeleteAsync(id);
			if (!ok) return NotFound();
			return Ok(new { message = "Deleted" });
		}

		[HttpGet("{id}/rubrics")]
		public async Task<IActionResult> GetRubricsByQuestionId([FromRoute] long id)
		{
			var items = await _rubricService.GetRubricByQuestionId(id);
			return Ok(items);
		}
	}
}
