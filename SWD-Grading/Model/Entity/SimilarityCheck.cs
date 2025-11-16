using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Entity
{
	[Table("SimilarityCheck")]
	public class SimilarityCheck
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
		public DateTime CheckedAt { get; set; }

		[Required]
		[Column(TypeName = "decimal(5,4)")]
		public decimal Threshold { get; set; }

		// FK → User.id (who triggered the check)
		[Required]
		public int CheckedByUserId { get; set; }

		[ForeignKey(nameof(CheckedByUserId))]
		public User CheckedByUser { get; set; } = null!;

		// Navigation property
		public ICollection<SimilarityResult> SimilarityResults { get; set; } = new List<SimilarityResult>();
	}
}

