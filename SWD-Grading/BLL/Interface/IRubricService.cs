using BLL.Model.Request.Rubric;
using BLL.Model.Response.Rubric;
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
	public interface IRubricService
	{

		Task<IEnumerable<RubricResponse>> GetRubricByQuestionId(long id);
		Task<RubricResponse> UpdateAsync(long id, UpdateRubricRequest request);
		Task DeleteAsync(long id);
		Task<RubricResponse> CreateAsync(long questionId, CreateRubricRequest request);
	}
}
