using Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
	[Table("ExamZip")]
	public class ExamZip
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		// FK -> Exam.id
		[Required]
		public long ExamId { get; set; }

		[ForeignKey(nameof(ExamId))]
		public Exam Exam { get; set; } = null!;

		[MaxLength(255)]
		public string? ZipName { get; set; }

		[MaxLength(500)]
		public string? ZipPath { get; set; }

		public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

		[MaxLength(500)]
		public string? ExtractedPath { get; set; }

		[Required]
		public ParseStatus ParseStatus { get; set; } = ParseStatus.PENDING;

		public string? ParseSummary { get; set; }
	}
}
