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
    public class GradeDetailRepository : GenericRepository<GradeDetail, long>, IGradeDetailRepository
    {
        private readonly SWDGradingDbContext _context;
        public GradeDetailRepository(SWDGradingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GradeDetail>> GetByGradeId(long id)
        {
            var gradeDetails = await _context.GradeDetails.Where(gd => gd.GradeId == id).ToListAsync();
            return gradeDetails;
        }
    }
}
