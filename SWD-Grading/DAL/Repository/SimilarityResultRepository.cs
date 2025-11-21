using DAL.Interface;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
	public class SimilarityResultRepository : GenericRepository<SimilarityResult, long>, ISimilarityResultRepository
	{
		private readonly SWDGradingDbContext _context;

		public SimilarityResultRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<List<SimilarityResult>> GetConfirmedPlagiarismAsync(long userId)
		{
			return await _context.SimilarityResults
				.Include(sr => sr.DocFile1)
					.ThenInclude(df => df.ExamStudent)
						.ThenInclude(es => es.Student)
				.Include(sr => sr.DocFile2)
					.ThenInclude(df => df.ExamStudent)
						.ThenInclude(es => es.Student)
				.Where(sr => sr.VerificationStatus == VerificationStatus.TeacherConfirmed_Similar && sr.TeacherVerifiedByUserId == userId)
				.OrderByDescending(sr => sr.TeacherVerifiedAt)
				.ToListAsync();
		}

	}
}
