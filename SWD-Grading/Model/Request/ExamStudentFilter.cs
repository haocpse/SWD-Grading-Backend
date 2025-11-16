using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Request
{
	public class ExamStudentFilter
	{
		public int Page { get; set; } = 1;
		public int Size { get; set; } = 10;
		public string? Status { get; set; } // "NOT_FOUND", "PARSED", "GRADED"
	}
}

