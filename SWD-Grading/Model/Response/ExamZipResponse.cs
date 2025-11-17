using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Response
{
	public class ExamZipResponse
	{
		public long ExamZipId { get; set; }
		public long ExamId { get; set; }
		public string? ExamCode { get; set; }
		public string? ZipName { get; set; }
		public DateTime UploadedAt { get; set; }
		public string ParseStatus { get; set; } = null!;
		public string? ParseSummary { get; set; }
		public int ProcessedCount { get; set; }
		public int TotalCount { get; set; }
	}
}


