using DAL.Interface;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
	public class ExamQuestionRepository : GenericRepository<ExamQuestion, long>, IExamQuestionRepository
	{
		private readonly SWDGradingDbContext _context;

		public ExamQuestionRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}
	}
}
