using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Response.Student
{
	public class StudentResponse
	{
		public long Id { get; set; }
		public string StudentCode { get; set; } = null!;
		public string? FullName { get; set; }
		public string? Email { get; set; }
		public string? ClassName { get; set; }
	}
}
