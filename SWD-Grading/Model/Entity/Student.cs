using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
	public class Student
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		[Required]
		[MaxLength(50)]
		public string StudentCode { get; set; } = null!;

		[MaxLength(255)]
		public string? FullName { get; set; }

		[MaxLength(100)]
		public string? Email { get; set; }

		[MaxLength(100)]
		public string? ClassName { get; set; }
	}
}
