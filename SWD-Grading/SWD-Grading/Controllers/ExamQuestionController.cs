using BLL.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SWD_Grading.Controllers
{
	[Route("api/exam-questions")]
	[ApiController]
	public class ExamQuestionController : ControllerBase
	{

		private readonly IExamQuestionService _examQuestionService;

		public ExamQuestionController(IExamQuestionService examQuestionService)
		{
			_examQuestionService = examQuestionService;
		}

	}
}
