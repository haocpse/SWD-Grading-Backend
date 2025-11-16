using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Model.Response.Exam;
using BLL.Model.Response.Rubric;

namespace BLL.Model.Response.ExamQuestion
{
	public class ExamQuestionResponse
	{
		public long Id { get; set; }
		public int QuestionNumber { get; set; }
		public string? QuestionText { get; set; }
		public decimal MaxScore { get; set; }
		public List<RubricResponse> Rubrics { get; set; }
	}
}
