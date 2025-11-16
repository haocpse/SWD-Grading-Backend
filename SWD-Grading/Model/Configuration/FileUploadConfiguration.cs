using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Configuration
{
	public class FileUploadConfiguration
	{
		public int MaxFileSizeMB { get; set; }
		public List<string> AllowedExtensions { get; set; } = new();
		public string TempStoragePath { get; set; } = null!;
	}
}

