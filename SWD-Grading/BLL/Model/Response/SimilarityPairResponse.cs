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
	
	/// <summary>
	/// File path of the first document (from DocFile.FilePath)
	/// </summary>
	public string? DocFile1Path { get; set; }
	
	/// <summary>
	/// File path of the second document (from DocFile.FilePath)
	/// </summary>
	public string? DocFile2Path { get; set; }
	}
}

