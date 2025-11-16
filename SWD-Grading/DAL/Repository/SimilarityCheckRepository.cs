using DAL.Interface;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repository
{
	public class SimilarityCheckRepository : GenericRepository<SimilarityCheck, long>, ISimilarityCheckRepository
	{
		private readonly SWDGradingDbContext _context;

		public SimilarityCheckRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<List<SimilarityCheck>> GetCheckHistoryByExamIdAsync(long examId)
		{
			return await _context.SimilarityChecks
				.Include(sc => sc.Exam)
				.Include(sc => sc.CheckedByUser)
				.Include(sc => sc.SimilarityResults)
				.Where(sc => sc.ExamId == examId)
				.OrderByDescending(sc => sc.CheckedAt)
				.ToListAsync();
		}

		public async Task<SimilarityCheck?> GetCheckByIdWithResultsAsync(long checkId)
		{
			return await _context.SimilarityChecks
				.Include(sc => sc.Exam)
				.Include(sc => sc.CheckedByUser)
				.Include(sc => sc.SimilarityResults)
					.ThenInclude(sr => sr.DocFile1)
						.ThenInclude(df => df.ExamStudent)
							.ThenInclude(es => es.Student)
				.Include(sc => sc.SimilarityResults)
					.ThenInclude(sr => sr.DocFile2)
						.ThenInclude(df => df.ExamStudent)
							.ThenInclude(es => es.Student)
				.FirstOrDefaultAsync(sc => sc.Id == checkId);
		}
	}
}

