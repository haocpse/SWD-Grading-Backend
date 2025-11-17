using DAL.Interface;
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
	}
}
