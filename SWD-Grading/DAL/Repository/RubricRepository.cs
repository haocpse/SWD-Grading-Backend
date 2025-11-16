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
	public class RubricRepository : GenericRepository<Rubric, long> , IRubricRepository
	{

		private readonly SWDGradingDbContext _context;

		public RubricRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<IEnumerable<Rubric>> GetRubricByQuestionId(long id)
		{
			return await _context.Rubrics
				.Where(q => q.ExamQuestionId == id)
				.OrderBy(q => q.OrderIndex)
				.ToListAsync();
		}
	}
}
