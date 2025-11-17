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
    public class GradeExport
    {

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		// FK → Exam.id
		[Required]
		public long ExamId { get; set; }

		[ForeignKey(nameof(ExamId))]
		public Exam Exam { get; set; } = null!;
		
		public string? Url { get; set; }

		public DateTime CreatedAt { get; set; }
		[Required]
		public int UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public User Teacher { get; set; } = null!;

		public bool IsFinal { get; set; }

	}
}
