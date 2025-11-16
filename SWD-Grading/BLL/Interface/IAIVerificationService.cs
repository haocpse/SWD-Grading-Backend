using System.Threading.Tasks;

namespace BLL.Interface
{
	public class AIVerificationResult
	{
		public bool IsSimilar { get; set; }
		public decimal ConfidenceScore { get; set; }
		public string Analysis { get; set; } = string.Empty;
		public string Summary { get; set; } = string.Empty;
	}

	public interface IAIVerificationService
	{
		Task<AIVerificationResult> VerifyTextSimilarityAsync(string text1, string text2, string student1Code, string student2Code);
	}
}

