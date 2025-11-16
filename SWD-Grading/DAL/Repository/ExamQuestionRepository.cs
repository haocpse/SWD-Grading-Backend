using DAL.Interface;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
	public class ExamQuestionRepository : GenericRepository<ExamQuestion, long>, IExamQuestionRepository
	{
		private readonly SWDGradingDbContext _context;

		public ExamQuestionRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<IEnumerable<ExamQuestion>> GetQuestionByExamId(long examId)
		{
			return await _context.ExamQuestions
				.Where(q => q.ExamId == examId)
				.OrderBy(q => q.QuestionNumber)
				.ToListAsync();
		}
	}
}
