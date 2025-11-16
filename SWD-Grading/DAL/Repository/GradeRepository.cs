using DAL.Interface;
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
    }
}
