using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using BLL.Interface;
using Microsoft.Extensions.Configuration;
using Model.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
	public class S3Service : IS3Service
	{
		private readonly IAmazonS3 _s3Client;
		private readonly AwsConfiguration _awsConfig;

		public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
		{
			_s3Client = s3Client;
			_awsConfig = new AwsConfiguration();
			configuration.GetSection("AWS").Bind(_awsConfig);
		}

		public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string path)
		{
			try
			{
				// Construct the full S3 key
				var s3Key = $"{path.TrimEnd('/')}/{fileName}";

				var uploadRequest = new TransferUtilityUploadRequest
				{
					InputStream = fileStream,
					Key = s3Key,
					BucketName = _awsConfig.BucketName,
					CannedACL = S3CannedACL.Private
				};

				var transferUtility = new TransferUtility(_s3Client);
				await transferUtility.UploadAsync(uploadRequest);

				// Return the S3 URL
				return $"https://{_awsConfig.BucketName}.s3.{_awsConfig.Region}.amazonaws.com/{s3Key}";
			}
			catch (AmazonS3Exception ex)
			{
				throw new Exception($"Error uploading file to S3: {ex.Message}", ex);
			}
			catch (Exception ex)
			{
				throw new Exception($"Unexpected error uploading file: {ex.Message}", ex);
			}
		}

		public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string path)
		{
			Console.WriteLine("============Extension: " + Path.GetExtension(fileName).ToLower());
			try
			{
				// Validate extension
				var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
				var ext = Path.GetExtension(fileName).ToLower();
				
				if (!allowedExtensions.Contains(ext))
				{
					throw new Exception("File không phải là hình ảnh hợp lệ.");
				}

				var s3Key = $"{path.TrimEnd('/')}/images/{fileName}";

				var uploadRequest = new TransferUtilityUploadRequest
				{
					InputStream = imageStream,
					Key = s3Key,
					BucketName = _awsConfig.BucketName,
					
					CannedACL = S3CannedACL.Private
				};

				var transferUtility = new TransferUtility(_s3Client);
				await transferUtility.UploadAsync(uploadRequest);

				return $"https://{_awsConfig.BucketName}.s3.{_awsConfig.Region}.amazonaws.com/{s3Key}";
			}
			catch (AmazonS3Exception ex)
			{
				throw new Exception($"Error uploading image to S3: {ex.Message}", ex);
			}
			catch (Exception ex)
			{
				throw new Exception($"Unexpected error uploading image: {ex.Message}", ex);
			}
		}

		public async Task<bool> DeleteFileAsync(string path)
		{
			try
			{
				var deleteRequest = new DeleteObjectRequest
				{
					BucketName = _awsConfig.BucketName,
					Key = path
				};

				await _s3Client.DeleteObjectAsync(deleteRequest);
				return true;
			}
			catch (AmazonS3Exception ex)
			{
				Console.WriteLine($"Error deleting file from S3: {ex.Message}");
				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Unexpected error deleting file: {ex.Message}");
				return false;
			}
		}

		public async Task<Stream> GetFileAsync(string path)
		{
			try
			{
				var request = new GetObjectRequest
				{
					BucketName = _awsConfig.BucketName,
					Key = path
				};

				var response = await _s3Client.GetObjectAsync(request);
				var memoryStream = new MemoryStream();
				await response.ResponseStream.CopyToAsync(memoryStream);
				memoryStream.Position = 0;
				return memoryStream;
			}
			catch (AmazonS3Exception ex)
			{
				throw new Exception($"Error getting file from S3: {ex.Message}", ex);
			}
			catch (Exception ex)
			{
				throw new Exception($"Unexpected error getting file: {ex.Message}", ex);
			}
		}
	}
}

