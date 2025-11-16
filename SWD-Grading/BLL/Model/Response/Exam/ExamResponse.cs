using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Response.Exam
{
	public class ExamResponse
	{
		public long Id { get; set; }
		public string ExamCode { get; set; }
		public string? Title { get; set; }
		public string? Description { get; set; }
	}
}
