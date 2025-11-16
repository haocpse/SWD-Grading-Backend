using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interface
{
	public interface IExamQuestionRepository : IGenericRepository<ExamQuestion, long>
	{

		Task<IEnumerable<ExamQuestion>> GetQuestionByExamId(long id);

	}
}
