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
	public class StudentRepository : GenericRepository<Student, long>, IStudentRepository
	{
		private readonly SWDGradingDbContext _context;

		public StudentRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<Student?> GetByStudentCodeAsync(string studentCode)
		{
			return await _context.Set<Student>()
				.FirstOrDefaultAsync(s => s.StudentCode == studentCode);
		}
	}
}

