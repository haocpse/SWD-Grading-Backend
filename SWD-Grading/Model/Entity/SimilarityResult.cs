using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Entity
{
	[Table("SimilarityResult")]
	public class SimilarityResult
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		// FK → SimilarityCheck.id
		[Required]
		public long SimilarityCheckId { get; set; }

		[ForeignKey(nameof(SimilarityCheckId))]
		public SimilarityCheck SimilarityCheck { get; set; } = null!;

		// FK → DocFile.id (first document)
		[Required]
		public long DocFile1Id { get; set; }

		[ForeignKey(nameof(DocFile1Id))]
		public DocFile DocFile1 { get; set; } = null!;

		// FK → DocFile.id (second document)
		[Required]
		public long DocFile2Id { get; set; }

		[ForeignKey(nameof(DocFile2Id))]
		public DocFile DocFile2 { get; set; } = null!;

		[Required]
		[Column(TypeName = "decimal(5,4)")]
		public decimal SimilarityScore { get; set; }

		// Denormalized fields for quick reference
		[MaxLength(50)]
		public string? Student1Code { get; set; }

		[MaxLength(50)]
		public string? Student2Code { get; set; }
	}
}

