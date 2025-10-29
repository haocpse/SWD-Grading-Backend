using BLL.Exceptions;
using BLL.Model.Response;
using System.Net;
using System.Text.Json;

namespace SWD_Grading.Exceptions
{
	public class GlobalExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<GlobalExceptionMiddleware> _logger;
		private readonly IWebHostEnvironment _env;

		public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment env)
		{
			_next = next;
			_logger = logger;
			_env = env;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unhandled exception occurred");

				int statusCode = (int)HttpStatusCode.InternalServerError;
				string message = "An unexpected error occurred.";
				object? details = null;

				if (ex is AppException appEx)
				{
					statusCode = appEx.StatusCode;
					message = appEx.Message;
				}
				else if (_env.IsDevelopment())
				{
					message = ex.Message;
					details = new
					{
						exception = ex.GetType().Name,
						stackTrace = ex.StackTrace,
						inner = ex.InnerException?.Message
					};
				}

				context.Response.StatusCode = statusCode;
				context.Response.ContentType = "application/json";

				var response = new BaseResponse<object>
				{
					Code = statusCode,
					Success = false,
					Message = message,
					Data = details
				};

				var jsonOptions = new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
					WriteIndented = _env.IsDevelopment()
				};

				await context.Response.WriteAsJsonAsync(response, jsonOptions);
			}
		}
	}
}
