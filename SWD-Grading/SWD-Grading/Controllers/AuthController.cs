using BLL.Interface;
using BLL.Model.Request;
using BLL.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SWD_Grading.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			var result = await _authService.LoginAsync(request);
			if (result == null)
			{
				return Unauthorized(new BaseResponse<object>
                {
                    Code = 400,
                    Success = false,
                    Message = "Username or password invalid",
                });
			}
            return Ok(new BaseResponse<LoginResponse>
            {
                Code = 200,
                Success = true,
                Message = "Login successfully",
                Data = result
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (result == null)
            {
                return BadRequest(new BaseResponse<object>
                {
                    Code = 400,
                    Success = false,
                    Message = "Username already exists",
                });
            }

            return Ok(new BaseResponse<RegisterResponse>
            {
                Code = 200,
                Success = true,
                Message = "Register successfully",
                Data = result
            });
        }

    }
}
