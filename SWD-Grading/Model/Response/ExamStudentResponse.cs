using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Response
{
	public class ExamStudentResponse
	{
		public long ExamStudentId { get; set; }
		public string StudentCode { get; set; } = null!;
		public string? StudentName { get; set; }
		public string Status { get; set; } = null!;
		public string? Note { get; set; }
		public List<DocFileResponse> DocFiles { get; set; } = new List<DocFileResponse>();
	}
}

