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

	}
}
