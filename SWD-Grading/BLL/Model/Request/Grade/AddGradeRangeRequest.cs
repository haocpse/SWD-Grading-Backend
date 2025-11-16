using Model.Entity;
using Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Request.Grade
{
	public class AddGradeRangeRequest
	{
		public long ExamStudentId { get; set; }
		public decimal TotalScore { get; set; } = 0;
		public string? Comment { get; set; } = "";
		public DateTime? GradedAt { get; set; } = DateTime.UtcNow;
		public string? GradedBy { get; set; }
		public int Attempt { get; set; } = 1;
		public GradeStatus Status { get; set; } = GradeStatus.CREATED;
	}
}
