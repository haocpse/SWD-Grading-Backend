namespace BLL.Model.Response
{
	public class SimilarityPairResponse
	{
		public long ResultId { get; set; }
		public string Student1Code { get; set; } = null!;
		public string Student2Code { get; set; } = null!;
		public string? DocFile1Name { get; set; }
		public string? DocFile2Name { get; set; }
		public long DocFile1Id { get; set; }
		public long DocFile2Id { get; set; }
		public decimal SimilarityScore { get; set; }
	}
}

