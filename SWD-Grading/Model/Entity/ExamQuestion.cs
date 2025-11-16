using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
	[Table("ExamQuestion")]
	public class ExamQuestion
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		// FK → Exam.id
		[Required]
		public long ExamId { get; set; }

		[ForeignKey(nameof(ExamId))]
		public Exam Exam { get; set; } = null!;

		[Required]
		public int QuestionNumber { get; set; }

		public string? QuestionText { get; set; } // TEXT

		[Column(TypeName = "DECIMAL(5,2)")]
		public decimal MaxScore { get; set; }

		[MaxLength(255)]
		public string? RelatedDocSection { get; set; }

		public List<Rubric> Rubrics { get; set; } = new();
	}
}
