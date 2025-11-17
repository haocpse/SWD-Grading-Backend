using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
	[Table("Exam")]
	public class Exam
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[Required]
		[MaxLength(50)]
		public string ExamCode { get; set; } = null!;

		[MaxLength(255)]
		public string? Title { get; set; }

		public string? Description { get; set; }

		public string? ExamPaper { get; set; }

		public string? OriginalExcel { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		public List<ExamQuestion> Questions { get; set; }
		public List<ExamStudent> ExamStudents { get; set; }
		public List<GradeExport> GradeExports { get; set; }
	}
}
