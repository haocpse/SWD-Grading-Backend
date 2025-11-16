using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Request
{
	public class UploadStudentSolutionsRequest
	{
		[Required]
		public long ExamId { get; set; }

		[Required]
		[MaxLength(50)]
		public string ExamCode { get; set; } = null!;
	}
}

