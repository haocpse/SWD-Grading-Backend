using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
	public interface IS3Service
	{
		/// <summary>
		/// Upload file to S3
		/// </summary>
		/// <param name="fileStream">File stream to upload</param>
		/// <param name="fileName">Name of the file</param>
		/// <param name="path">S3 path (e.g., ExamCode/StudentCode/)</param>
		/// <returns>S3 URL of uploaded file</returns>
		Task<string> UploadFileAsync(Stream fileStream, string fileName, string path);

		/// <summary>
		/// Delete file from S3
		/// </summary>
		/// <param name="path">Full S3 path to the file</param>
		/// <returns>True if successful</returns>
		Task<bool> DeleteFileAsync(string path);

		/// <summary>
		/// Get file stream from S3
		/// </summary>
		/// <param name="path">Full S3 path to the file</param>
		/// <returns>File stream</returns>
		Task<Stream> GetFileAsync(string path);


		Task<string> UploadImageAsync(Stream fileStream, string fileName, string path);
	}
}

