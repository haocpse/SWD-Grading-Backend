using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interface
{
	public class SimilarityPair
	{
		public long DocFile1Id { get; set; }
		public long DocFile2Id { get; set; }
		public float SimilarityScore { get; set; }
	}

	public interface IVectorService
	{
		Task<float[]> GenerateEmbeddingAsync(string text);
		Task IndexDocumentAsync(long docFileId, long examId, string studentCode, string text);
		Task<List<SimilarityPair>> SearchSimilarDocumentsAsync(long examId, float threshold);
		Task<bool> IsDocumentIndexedAsync(long docFileId);
		Task EnsureCollectionExistsAsync();
	}
}

