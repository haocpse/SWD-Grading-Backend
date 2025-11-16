using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Request.ExamQuestion
{
	public class UpdateExamQuestionRequest
	{
		public int QuestionNumber { get; set; }
		public string? QuestionText { get; set; }
		public decimal MaxScore { get; set; }
		public string? RelatedDocSection { get; set; }
	}
}
