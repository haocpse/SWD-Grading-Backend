using System.ComponentModel.DataAnnotations;

namespace BLL.Model.Request
{
	public class CheckPlagiarismRequest
	{
		[Required]
		[Range(0.0, 1.0, ErrorMessage = "Threshold must be between 0.0 and 1.0")]
		public decimal Threshold { get; set; } = 0.8m;
	}
}



