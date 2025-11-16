using DAL.Interface;
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
	public class ExamRepository : GenericRepository<Exam, long>, IExamRepository
	{
		private readonly SWDGradingDbContext _context;
		public ExamRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<Exam?> GetByIdAsync(long id)
		{
			return await _context.Set<Exam>()
				.FirstOrDefaultAsync(e => e.Id == id);
		}

		public async Task<Exam?> GetByExamCodeAsync(string examCode)
		{
			return await _context.Set<Exam>()
				.FirstOrDefaultAsync(e => e.ExamCode == examCode);
		}
	}
}

