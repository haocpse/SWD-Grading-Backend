using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
	[Table("Rubric")]
	public class Rubric
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		// FK → ExamQuestion.id
		[Required]
		public long ExamQuestionId { get; set; }

		[ForeignKey(nameof(ExamQuestionId))]
		public ExamQuestion ExamQuestion { get; set; } = null!;

		[Required]
		[MaxLength(255)]
		public string Criterion { get; set; } = null!;

		[Column(TypeName = "DECIMAL(5,2)")]
		public decimal MaxScore { get; set; }

		// JSON TEXT rule
		public string? AutoCheckRule { get; set; }

		public int OrderIndex { get; set; }
	}
}
