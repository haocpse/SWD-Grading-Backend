using BLL.Exceptions;
using DocumentFormat.OpenXml.Office2010.Excel;
using Model.Entity;
using Model.Enums;
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
				throw new AppException("UserId claim not found");

			return int.Parse(id);
		}

		public static UserRole GetUserRole(this ClaimsPrincipal user) 
		{
			var role = user.FindFirst(ClaimTypes.Role)?.Value;
			if (string.IsNullOrEmpty(role))
				throw new AppException("Role claim not found");

			if (!Enum.TryParse<UserRole>(role, out var userRole))
				throw new AppException("Invalid role value in token");

			return userRole;
		}
	}
}
