using BLL.Model.Request.ExamQuestion;
using BLL.Model.Response.ExamQuestion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
	public interface IExamQuestionService
	{
		Task<ExamQuestionResponse?> GetByIdAsync(long id);
		Task<ExamQuestionResponse?> UpdateAsync(long id, UpdateExamQuestionRequest request);
		Task<bool> DeleteAsync(long id);
	}
}
