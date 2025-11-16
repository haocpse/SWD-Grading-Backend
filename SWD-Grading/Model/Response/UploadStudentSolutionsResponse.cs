using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Response
{
	public class UploadStudentSolutionsResponse
	{
		public long ExamZipId { get; set; }
		public string Status { get; set; } = null!;
		public string Message { get; set; } = null!;
	}
}

