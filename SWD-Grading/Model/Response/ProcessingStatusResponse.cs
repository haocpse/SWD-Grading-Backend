using Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Response
{
	public class ProcessingStatusResponse
	{
		public long ExamZipId { get; set; }
		public ParseStatus ParseStatus { get; set; }
		public int ProcessedCount { get; set; }
		public int TotalCount { get; set; }
		public List<string> Errors { get; set; } = new();
		public List<string> FailedStudents { get; set; } = new();
		public string? ParseSummary { get; set; }
	}
}

