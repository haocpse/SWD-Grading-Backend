using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
	[Table("GradeDetail")]
	public class GradeDetail
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		// FK → Grade.id
		[Required]
		public long GradeId { get; set; }

		[ForeignKey(nameof(GradeId))]
		public Grade Grade { get; set; } = null!;

		// FK → Rubric.id
		[Required]
		public long RubricId { get; set; }

		[ForeignKey(nameof(RubricId))]
		public Rubric Rubric { get; set; } = null!;

		[Column(TypeName = "DECIMAL(5,2)")]
		public decimal Score { get; set; }

		public string? Comment { get; set; } // TEXT

		public string? AutoDetectResult { get; set; } // TEXT
	}
}
