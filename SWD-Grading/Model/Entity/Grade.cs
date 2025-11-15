using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
	[Table("Grade")]
	public class Grade
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		// FK → ExamStudent.id
		[Required]
		public long ExamStudentId { get; set; }

		[ForeignKey(nameof(ExamStudentId))]
		public ExamStudent ExamStudent { get; set; } = null!;

		[Column(TypeName = "DECIMAL(5,2)")]
		public decimal TotalScore { get; set; }

		public string? Comment { get; set; }  // TEXT

		public DateTime? GradedAt { get; set; }

		[MaxLength(100)]
		public string? GradedBy { get; set; }
	}
}
