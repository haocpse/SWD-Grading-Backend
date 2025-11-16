using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Request.Rubric
{
	public class UpdateRubricRequest
	{ 
		public string Criterion { get; set; } = null!;
		public decimal MaxScore { get; set; }
		public string? AutoCheckRule { get; set; }
		public int OrderIndex { get; set; }
	}
}
