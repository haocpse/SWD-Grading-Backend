using Model.Enums;
using System;

namespace BLL.Model.Response
{
	public class VerificationResponse
	{
		public long SimilarityResultId { get; set; }
		public string Student1Code { get; set; } = string.Empty;
		public string Student2Code { get; set; } = string.Empty;
		public decimal SimilarityScore { get; set; }
		public VerificationStatus VerificationStatus { get; set; }
		public string VerificationStatusText { get; set; } = string.Empty;
		
		// AI Verification
		public bool? AIVerifiedSimilar { get; set; }
		public decimal? AIConfidenceScore { get; set; }
		public string? AISummary { get; set; }
		public string? AIAnalysis { get; set; }
		public DateTime? AIVerifiedAt { get; set; }
		
		// Teacher Verification
		public bool? TeacherVerifiedSimilar { get; set; }
		public string? TeacherUsername { get; set; }
		public string? TeacherNotes { get; set; }
		public DateTime? TeacherVerifiedAt { get; set; }
	}
}



