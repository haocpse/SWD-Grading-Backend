using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Request.Exam
{
	public class CreateExamRequest
	{
		public string ExamCode { get; set; }
		public string? Title { get; set; }
	}
}
