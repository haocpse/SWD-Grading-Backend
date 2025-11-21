using Model.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Response.Grade
{
	public class GradeExportResponse
	{
		public long Id { get; set; }
		public long ExamId { get; set; }

		public string? Url { get; set; }

		public DateTime CreatedAt { get; set; }
		public string TeacherCode { get; set; }

		public bool IsFinal { get; set; }
	}
}
