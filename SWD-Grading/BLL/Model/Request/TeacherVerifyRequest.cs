using System.ComponentModel.DataAnnotations;

namespace BLL.Model.Request
{
	public class TeacherVerifyRequest
	{
		[Required]
		public bool IsSimilar { get; set; }

		[MaxLength(500)]
		public string? Notes { get; set; }
	}
}



