using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Exceptions
{
	public class AppException : Exception
	{
		public int StatusCode { get; }
		public AppException(string message, int statusCode = 400) : base(message)
		{
			StatusCode = statusCode;
		}
	}
}
