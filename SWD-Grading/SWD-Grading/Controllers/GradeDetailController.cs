using BLL.Interface;
using BLL.Model.Request.Grade;
using BLL.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SWD_Grading.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradeDetailController : ControllerBase
    {
        private readonly IGradeDetailService _gradeDetailService;
        public GradeDetailController(IGradeDetailService gradeDetailService)
        {
            _gradeDetailService = gradeDetailService;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGradeDetail(long id, [FromBody] GradeDetailRequest request)
        {
            await _gradeDetailService.Update(request, id);
            var response = new BaseResponse<object>
            {
                Code = 200,
                Success = true,
                Message = "Grade detail updated successfully",
            };
            return Ok(response);
        }
    }
}
