using BLL.Interface;
using DAL.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Model.Configuration;
using Model.Entity;
using Model.Enums;
using Model.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
	public class ExamUploadService : IExamUploadService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IConfiguration _configuration;
		private readonly FileUploadConfiguration _fileUploadConfig;

		public ExamUploadService(IUnitOfWork unitOfWork, IConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_configuration = configuration;
			_fileUploadConfig = new FileUploadConfiguration();
			configuration.GetSection("FileUpload").Bind(_fileUploadConfig);
		}

		public async Task<long> InitiateUploadAsync(IFormFile zipFile, long examId)
		{
			// Validate file
			if (zipFile == null || zipFile.Length == 0)
			{
				throw new ArgumentException("File is empty or null");
			}

			// Check file extension
			var extension = Path.GetExtension(zipFile.FileName).ToLower();
			if (extension != ".zip")
			{
				throw new ArgumentException($"File type {extension} is not allowed. Only .zip files are accepted.");
			}

			// Check file size
			var maxSizeBytes = _fileUploadConfig.MaxFileSizeMB * 1024 * 1024;
			if (zipFile.Length > maxSizeBytes)
			{
				throw new ArgumentException($"File size exceeds maximum allowed size of {_fileUploadConfig.MaxFileSizeMB}MB");
			}

			// Verify exam exists and get exam code
			var exam = await _unitOfWork.ExamRepository.GetByIdAsync(examId);
			if (exam == null)
			{
				throw new ArgumentException($"Exam with ID {examId} not found");
			}

			var examCode = exam.ExamCode;

			// Create temp storage directory for this upload
			var tempStoragePath = _fileUploadConfig.TempStoragePath;
			if (!Path.IsPathRooted(tempStoragePath))
			{
				tempStoragePath = Path.Combine(Directory.GetCurrentDirectory(), tempStoragePath);
			}

			if (!Directory.Exists(tempStoragePath))
			{
				Directory.CreateDirectory(tempStoragePath);
			}

			// Save ZIP file temporarily
			var fileName = $"{examCode}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}.zip";
			var zipFilePath = Path.Combine(tempStoragePath, fileName);

			using (var stream = new FileStream(zipFilePath, FileMode.Create))
			{
				await zipFile.CopyToAsync(stream);
			}

			// Create ExamZip record
			var examZip = new ExamZip
			{
				ExamId = examId,
				ZipName = zipFile.FileName,
				ZipPath = zipFilePath, // Store ZIP file path
				UploadedAt = DateTime.UtcNow,
				ExtractedPath = null, // Will be set during processing
				ParseStatus = ParseStatus.PENDING,
				ParseSummary = "Waiting for processing..."
			};

			await _unitOfWork.ExamZipRepository.AddAsync(examZip);
			await _unitOfWork.SaveChangesAsync();

			return examZip.Id;
		}

		public async Task<ProcessingStatusResponse> GetProcessingStatusAsync(long examZipId)
		{
			var examZip = await _unitOfWork.ExamZipRepository.GetByIdAsync(examZipId);
			if (examZip == null)
			{
				throw new ArgumentException($"ExamZip with ID {examZipId} not found");
			}

			// Get exam students associated with this exam zip
			var examStudents = await _unitOfWork.ExamStudentRepository.GetByExamZipIdAsync(examZipId);

			// Count processed students
			var processedCount = examStudents.Count(es => es.Status != ExamStudentStatus.NOT_FOUND);
			var totalCount = examStudents.Count;

			// Get failed students
			var failedStudents = examStudents
				.Where(es => es.Status == ExamStudentStatus.NOT_FOUND)
				.Select(es => es.Student?.StudentCode ?? "Unknown")
				.ToList();

			// Parse errors from summary
			var errors = new List<string>();
			if (!string.IsNullOrEmpty(examZip.ParseSummary))
			{
				var lines = examZip.ParseSummary.Split('\n');
				errors = lines.Where(l => l.Contains("Error")).ToList();
			}

			return new ProcessingStatusResponse
			{
				ExamZipId = examZipId,
				ParseStatus = examZip.ParseStatus,
				ProcessedCount = processedCount,
				TotalCount = totalCount,
				Errors = errors,
				FailedStudents = failedStudents,
				ParseSummary = examZip.ParseSummary
			};
		}
	}
}

