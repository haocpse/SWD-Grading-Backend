using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interface
{
	public interface IDocFileRepository : IGenericRepository<DocFile, long>
	{
		Task BulkInsertAsync(List<DocFile> docFiles);
		Task<List<DocFile>> GetByExamStudentIdAsync(long examStudentId);
	}
}

