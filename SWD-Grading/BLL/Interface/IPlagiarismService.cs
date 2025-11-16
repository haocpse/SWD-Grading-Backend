using BLL.Model.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interface
{
	public interface IPlagiarismService
	{
		Task<PlagiarismCheckResponse> CheckPlagiarismAsync(long examId, decimal threshold, int userId);
		Task<List<PlagiarismCheckResponse>> GetCheckHistoryAsync(long examId);
		Task GenerateEmbeddingForDocFileAsync(long docFileId);
	}
}

