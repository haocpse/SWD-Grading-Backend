using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
	public interface IFileProcessingService
	{
		/// <summary>
		/// Process student solutions from uploaded ZIP file
		/// </summary>
		/// <param name="examZipId">ID of the ExamZip record</param>
		Task ProcessStudentSolutionsAsync(long examZipId);

		/// <summary>
		/// Extract text content from Word document
		/// </summary>
		/// <param name="wordFilePath">Path to the Word document</param>
		/// <returns>Extracted text content</returns>
		string ExtractTextFromWord(string wordFilePath);
	}
}

