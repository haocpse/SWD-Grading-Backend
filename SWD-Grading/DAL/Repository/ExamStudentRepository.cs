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
	public class ExamStudentRepository : GenericRepository<ExamStudent, long>, IExamStudentRepository
	{
		private readonly SWDGradingDbContext _context;

		public ExamStudentRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task BulkInsertAsync(List<ExamStudent> examStudents)
		{
			await _context.Set<ExamStudent>().AddRangeAsync(examStudents);
		}

		public async Task<ExamStudent?> GetByExamAndStudentAsync(long examId, long studentId)
		{
			return await _context.Set<ExamStudent>()
				.Include(es => es.Student)
				.Include(es => es.Exam)
				.FirstOrDefaultAsync(es => es.ExamId == examId && es.StudentId == studentId);
		}

		public async Task<List<ExamStudent>> GetByExamZipIdAsync(long examZipId)
		{
			// Get ExamStudent records associated with a specific ExamZip through DocFile relationship
			return await _context.Set<ExamStudent>()
				.Include(es => es.Student)
				.Where(es => _context.Set<DocFile>()
					.Any(df => df.ExamStudentId == es.Id && df.ExamZipId == examZipId))
				.ToListAsync();
		}
	}
}

