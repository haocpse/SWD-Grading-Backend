using BLL.Interface;
using BLL.Model.Request;
using BLL.Model.Response;
using BLL.Model.Response.Grade;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SWD_Grading.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradeController : ControllerBase
    {
        private readonly IGradeService _gradeService;
        public GradeController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGrades([FromQuery] PagedRequest request)
        {
            var grades = await _gradeService.GetAll(request);

            var response = new BaseResponse<PagingResponse<GradeResponse>>
            {
                Success = true,
                Message = "Grades retrieved successfully",
                Data = grades
            };

            return Ok(response);
        }
    }
}
