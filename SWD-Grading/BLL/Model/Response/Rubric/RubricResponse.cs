using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Response.Rubric
{
	public class RubricResponse
	{
		public long Id { get; set; }
		public long ExamQuestionId { get; set; }
		public string Criterion { get; set; } = null!;
		public decimal MaxScore { get; set; }
		public string? AutoCheckRule { get; set; }
		public int OrderIndex { get; set; }

	}
}
