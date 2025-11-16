using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Response
{
	public class PagingResponse<T>
	{
		public IEnumerable<T> Result { get; set; }
		public int Page { get; set; }
		public int Size { get; set; }
		public int TotalItems { get; set; }
		public int TotalPages { get; set; }
		public int CurrentItems => Result?.Count() ?? 0;
	}
}
