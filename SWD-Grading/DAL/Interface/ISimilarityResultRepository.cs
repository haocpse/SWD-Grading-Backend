using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interface
{
	public interface ISimilarityResultRepository : IGenericRepository<SimilarityResult, long>
	{
		Task<List<SimilarityResult>> GetConfirmedPlagiarismAsync(long userId);
	}
}
