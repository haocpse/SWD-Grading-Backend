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
	public class ExamZipRepository : GenericRepository<ExamZip, long>, IExamZipRepository
	{
		private readonly SWDGradingDbContext _context;

		public ExamZipRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<ExamZip?> GetByIdAsync(long id)
		{
			return await _context.Set<ExamZip>()
				.Include(ez => ez.Exam)
				.FirstOrDefaultAsync(ez => ez.Id == id);
		}

		public async Task<List<ExamZip>> GetPendingExamZipsAsync()
		{
			return await _context.Set<ExamZip>()
				.Include(ez => ez.Exam)
				.Where(ez => ez.ParseStatus == ParseStatus.PENDING)
				.OrderBy(ez => ez.UploadedAt)
				.ToListAsync();
		}
	}
}

