using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Request.Student
{
	public class CreateStudentRequest
	{

		public string StudentCode { get; set; } = "";
		public string FullName { get; set; } = "";
		public string Email { get; set; } = "";

	}
}
