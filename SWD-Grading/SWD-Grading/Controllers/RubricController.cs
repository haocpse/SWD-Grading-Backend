using BLL.Interface;
using BLL.Model.Request.Rubric;
using BLL.Model.Response.Rubric;
using BLL.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace SWD_Grading.Controllers
{
	[Route("api/rubrics")]
	[ApiController]
	[Authorize]
	public class RubricController : ControllerBase
	{
		private readonly IRubricService _service;

		public RubricController(IRubricService service)
		{
			_service = service;
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateRubricRequest request)
		{
			var result = await _service.UpdateAsync(id, request);

			BaseResponse<RubricResponse?> response = new()
			{
				Code = 200,
				Message = "Update rubric successfully",
				Data = result
			};

			return Ok(response);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(long id)
		{
			await _service.DeleteAsync(id);

			BaseResponse<string?> response = new()
			{
				Code = 200,
				Message = "Delete rubric successfully",
				Data = null
			};

			return NoContent();
		}

	}
}
