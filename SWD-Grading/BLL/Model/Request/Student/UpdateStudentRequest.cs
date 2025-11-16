using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Request.Student
{
	public class UpdateStudentRequest
	{
		public string StudentCode { get; set; } = null!;
		public string? FullName { get; set; }
		public string? Email { get; set; }
	}
}
