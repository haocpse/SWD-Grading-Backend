using DAL.Interface;
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
    }
}
