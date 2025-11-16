using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interface
{
	public interface IExamStudentRepository : IGenericRepository<ExamStudent, long>
	{
		Task BulkInsertAsync(List<ExamStudent> examStudents);
		Task<ExamStudent?> GetByExamAndStudentAsync(long examId, long studentId);
		Task<List<ExamStudent>> GetByExamZipIdAsync(long examZipId);
	}
}

