using BLL.Model.Request.Student;
using BLL.Model.Response.Exam;
using BLL.Model.Response;
using BLL.Model.Response.Student;
using BLL.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
	public interface IStudentService
	{

		Task<StudentResponse?> GetByIdAsync(long id);
		Task<PagingResponse<StudentResponse>> GetAllAsync(StudentFilter filter);
		Task<StudentResponse> CreateAsync(CreateStudentRequest request);
		Task<StudentResponse?> UpdateAsync(long id, UpdateStudentRequest request);
		Task<bool> DeleteAsync(long id);

	}
}
