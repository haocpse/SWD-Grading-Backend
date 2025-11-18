using Model.Entity;
using Model.Enums;
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
		Task<List<ExamStudent>> GetByExamIdWithDetailsAsync(long examId, int skip, int take, ExamStudentStatus? statusFilter = null);
		Task<int> CountByExamIdAsync(long examId, ExamStudentStatus? statusFilter = null);
		Task<List<ExamStudent>> GetExamStudentByExamId(int teacherId, long examId);

		Task<int> CountByExamIdAndTeacherIdAsync(long examId, int teacherId, ExamStudentStatus? statusFilter = null);
		Task<List<ExamStudent>> GetByExamIdAndTeacherIdWithDetailsAsync(long examId, int teacherId, int skip, int take, ExamStudentStatus? statusFilter = null);
	}
}

