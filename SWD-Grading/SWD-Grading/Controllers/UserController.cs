using BLL.Interface;
using BLL.Model.Response.Exam;
using BLL.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SWD_Grading.Helper;
using BLL.Model.Request.Exam;
using Model.Response;
using Model.Request;

namespace SWD_Grading.Controllers
{
	[Route("api/me")]
	[ApiController]
	public class UserController : ControllerBase
	{

		private readonly IExamService _examService;
		private readonly IExamStudentService _examStudentService;
		public UserController(IExamService examService, IExamStudentService examStudentService)
		{
			_examService = examService;
			_examStudentService = examStudentService;
		}

		[HttpGet("/exams")]
		public async Task<IActionResult> GetMyExams([FromQuery] ExamFilter filter)
		{
			int userId = User.GetUserId();
			var result = await _examService.GetAssignedExam(filter, userId);
			BaseResponse<PagingResponse<ExamResponse>> response = new()
			{
				Code = 200,
				Message = "Get all exams successfully",
				Data = result
			};

			return Ok(response);
		}

		[HttpGet("/exams/{id}/exam-students")]
		public async Task<IActionResult> GetMyExamStudents([FromRoute] long id, [FromQuery] ExamStudentFilter filter)
		{
			int userId = User.GetUserId();
			var result = await _examStudentService.GetAssignedExamStudent(userId, id, filter);

			BaseResponse<PagingResponse<ExamStudentResponse>> response = new()
			{
				Code = 200,
				Message = "Get exam students successfully",
				Data = result
			};

			return Ok(response);
		}


	}
}
