using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Configuration
{
	public class AwsConfiguration
	{
		public string BucketName { get; set; } = null!;
		public string Region { get; set; } = null!;
		public string AccessKey { get; set; } = null!;
		public string SecretKey { get; set; } = null!;
	}
}

