using Model.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interface
{
	public interface ISimilarityCheckRepository : IGenericRepository<SimilarityCheck, long>
	{
		Task<List<SimilarityCheck>> GetCheckHistoryByExamIdAsync(long examId);
		Task<SimilarityCheck?> GetCheckByIdWithResultsAsync(long checkId);
	}
}

