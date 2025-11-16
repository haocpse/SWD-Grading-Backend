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
    public class GradeRepository : GenericRepository<Grade, long>, IGradeRepository
    {
        private readonly SWDGradingDbContext _context;

        public GradeRepository(SWDGradingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Grade>> GetAll()
        {
            var grade = await _context.Grades.ToListAsync();
            return grade;
        }

        public async Task<Grade?> GetById(long id)
        {
            return await _context.Grades.Include(g => g.Details)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<Grade>> GetByExamStudentId(long examStudentId)
        {
            return await _context.Grades
                .AsNoTracking()
                .Where(g => g.ExamStudentId == examStudentId).ToListAsync();
        }
    }
}
