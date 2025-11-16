using BLL.Interface;
using BLL.Model.Request;
using BLL.Model.Request.Grade;
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
                Code = 200,
                Success = true,
                Message = "Grades retrieved successfully",
                Data = grades
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGradeById(long id)
        {
            var gradeDetail = await _gradeService.GetById(id);
            if (gradeDetail == null)
            {
                return NotFound(new BaseResponse<object>
                {
                    Code = 404,
                    Success = false,
                    Message = "Grade not found",
                });
            }
            var response = new BaseResponse<GradeDetailResponse>
            {
                Code = 200,
                Success = true,
                Message = "Grade detail retrieved successfully",
                Data = gradeDetail
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGrade([FromBody] GradeRequest request)
        {
           await _gradeService.Create(request);
           var response = new BaseResponse<object>
            {
                Code = 201,
                Success = true,
                Message = "Grade created successfully",
           };
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGrade(long id, [FromBody] GradeRequest request)
        {
            try
            {
                await _gradeService.Update(request, id);
                var response = new BaseResponse<object>
                {
                    Code = 200,
                    Success = true,
                    Message = "Grade updated successfully",
                };
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new BaseResponse<object>
                {
                    Code = 404,
                    Success = false,
                    Message = "Grade not found",
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGrade(long id)
        {
            try
            {
                await _gradeService.Delete(id);
                var response = new BaseResponse<object>
                {
                    Code = 200,
                    Success = true,
                    Message = "Grade deleted successfully",
                };
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new BaseResponse<object>
                {
                    Code = 404,
                    Success = false,
                    Message = "Grade not found",
                });
            }
        }
    }
}
