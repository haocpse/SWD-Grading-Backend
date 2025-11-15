using Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
	[Table("ExamStudent")]
	public class ExamStudent
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		// FK → Exam.id
		[Required]
		public long ExamId { get; set; }

		[ForeignKey(nameof(ExamId))]
		public Exam Exam { get; set; } = null!;

		// FK → Student.id
		[Required]
		public long StudentId { get; set; }

		[ForeignKey(nameof(StudentId))]
		public Student Student { get; set; } = null!;

		[Required]
		public ExamStudentStatus Status { get; set; }

		public string? Note { get; set; }
	}

}
