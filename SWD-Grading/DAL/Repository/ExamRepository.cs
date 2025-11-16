using DAL.Interface;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
	public class ExamRepository : GenericRepository<Exam, long>, IExamRepository
	{
		private readonly SWDGradingDbContext _context;
		public ExamRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}
	}
}
