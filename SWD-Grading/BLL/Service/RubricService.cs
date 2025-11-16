using BLL.Interface;
using BLL.Model.Response.Rubric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
	public class RubricService : IRubricService
	{
		public Task<IEnumerable<RubricResponse>> GetRubricByQuestionId(long id)
		{
			throw new NotImplementedException();
		}
	}
}
