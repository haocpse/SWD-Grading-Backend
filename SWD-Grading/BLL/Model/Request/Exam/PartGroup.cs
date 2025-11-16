using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Model.Request.Exam
{
	public class PartGroup
	{
		public string PartName { get; set; } = "";
		public List<string> Descriptions { get; set; } = new();
	}
}
