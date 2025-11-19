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
	public class GradeExportRepository : GenericRepository<GradeExport, long>, IGradeExportRepository
	{
		private readonly SWDGradingDbContext _context;

		public GradeExportRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<List<GradeExport>> GetGradeExportByExamId(long examId)
		{
			return await _context.GradeExports
				.Include(ge => ge.Teacher)
				.Where(ge => ge.ExamId == examId)
				.ToListAsync();
		}
	}
}
