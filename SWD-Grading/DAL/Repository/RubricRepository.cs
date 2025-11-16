using DAL.Interface;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
	public class RubricRepository : GenericRepository<Rubric, long> , IRubricRepository
	{

		private readonly SWDGradingDbContext _context;

		public RubricRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}

	}
}
