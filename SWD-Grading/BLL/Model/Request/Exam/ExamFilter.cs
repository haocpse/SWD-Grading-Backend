using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Request.Exam
{
	public class ExamFilter
	{
		public int Page { get; set; } = 1;
		public int Size { get; set; } = 10;
	}
}
