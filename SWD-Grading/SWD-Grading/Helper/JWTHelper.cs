using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SWD_Grading.Helper
{
	public static class JWTHelper
	{
		public static int GetUserId(this ClaimsPrincipal user)
		{
			var id = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
					 ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(id))
				throw new Exception("UserId claim not found");

			return int.Parse(id);
		}
	}
}
