using BLL.Model.Response;
using Model.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interface
{
	public interface IPlagiarismService
	{
		Task<PlagiarismCheckResponse> CheckSuspiciousDocumentAsync(long docFileId, decimal threshold, int userId);
		Task GenerateEmbeddingForDocFileAsync(long docFileId);
		Task<VerificationResponse> VerifyWithAIAsync(long similarityResultId);
		Task<VerificationResponse> TeacherVerifyAsync(long similarityResultId, bool isSimilar, string? notes, int userId);

		Task<List<VerificationResponse>> GetConfirmedPlagiarismAsync(long userId);

	}
}

