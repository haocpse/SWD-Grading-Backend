using BLL.Model.Response;
using Model.Entity;
using Model.Request;
using Model.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
	public interface IExamStudentService
	{
		Task<PagingResponse<ExamStudentResponse>> GetExamStudentsByExamIdAsync(long examId, ExamStudentFilter filter);
		Task<PagingResponse<ExamStudentResponse>> GetAssignedExamStudent(int userId, long examId, ExamStudentFilter filter);
	}
}



