using BLL.Interface;
using BLL.Model.Request.Student;
using BLL.Model.Response.Student;
using BLL.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BLL.Service;
using Microsoft.AspNetCore.Authorization;

namespace SWD_Grading.Controllers
{
	[Route("api/students")]
	[ApiController]
	[Authorize]
	public class StudentController : ControllerBase
	{
		private readonly IStudentService _service;

		public StudentController(IStudentService service)
		{
			_service = service;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(long id)
		{
			var item = await _service.GetByIdAsync(id);

			return Ok(new BaseResponse<StudentResponse?>
			{
				Code = 200,
				Message = "Get student successfully",
				Data = item
			});
		}

		[HttpGet]
		public async Task<IActionResult> GetAllStudents([FromQuery] StudentFilter filter)
		{
			var result = await _service.GetAllAsync(filter);

			BaseResponse<PagingResponse<StudentResponse>> response = new()
			{
				Code = 200,
				Message = "Get all students successfully",
				Data = result
			};

			return Ok(response);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
		{
			var item = await _service.CreateAsync(request);

			return StatusCode(201, new BaseResponse<StudentResponse?>
			{
				Code = 201,
				Message = "Create student successfully",
				Data = item
			});
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(long id, [FromBody] UpdateStudentRequest request)
		{
			var item = await _service.UpdateAsync(id, request);

			return Ok(new BaseResponse<StudentResponse?>
			{
				Code = 200,
				Message = "Update student successfully",
				Data = item
			});
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(long id)
		{
			await _service.DeleteAsync(id);
			return NoContent();
		}
	}
}
