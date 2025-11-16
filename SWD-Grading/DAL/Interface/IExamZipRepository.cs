using Model.Entity;
using Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interface
{
	public interface IExamZipRepository : IGenericRepository<ExamZip, long>
	{
		Task<ExamZip?> GetByIdAsync(long id);
		Task<List<ExamZip>> GetPendingExamZipsAsync();
	}
}

