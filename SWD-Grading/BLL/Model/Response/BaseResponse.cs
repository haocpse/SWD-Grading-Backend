using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BLL.Model.Response
{
	public class BaseResponse<T>
	{
		public int Code { get; set; }
		public bool Success { get; set; }
		public string Message { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public T Data { get; set; }
	}
}
