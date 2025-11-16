using System;
using System.Collections.Generic;

namespace BLL.Model.Response
{
	public class PlagiarismCheckResponse
	{
		public long CheckId { get; set; }
		public long ExamId { get; set; }
		public string? ExamCode { get; set; }
		public DateTime CheckedAt { get; set; }
		public decimal Threshold { get; set; }
		public int TotalPairsChecked { get; set; }
		public int SuspiciousPairsCount { get; set; }
		public string CheckedByUsername { get; set; } = null!;
		public List<SimilarityPairResponse> SuspiciousPairs { get; set; } = new();
	}
}

