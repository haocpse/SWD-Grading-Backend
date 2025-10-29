using BLL.Interface;
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

	}
}
