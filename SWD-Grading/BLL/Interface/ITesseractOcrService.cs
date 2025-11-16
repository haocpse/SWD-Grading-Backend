using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
	public interface ITesseractOcrService
	{

		Task<string> ExtractText(long examId, string imagePath, IFormFile file, string language = "eng");

	}
}
