using BLL.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWD_Grading.Helper;

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

		//[HttpGet("/exams")]
		//public async Task<IActionResult> GetMyExams()
		//{
		//	int userId = User.GetUserId();
		//	var response = await _examService.GetAssignedExam(userId);
		//}

		//[HttpGet("/exams/{id}/exam-students")]
		//public async Task<IActionResult> GetMyExams([FromRoute] long id)
		//{
		//	int userId = User.GetUserId();
		//	var response = await _examStudentService.GetAssignedExamStudent(userId, id);
		//}


	}
}
