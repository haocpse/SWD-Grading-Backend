using BLL.Interface;
using DAL.Interface;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tesseract;

namespace BLL.Service
{
	public class TesseractOcrService : ITesseractOcrService
	{
		private readonly string _tessdataPath;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IS3Service _s3Service;

		public TesseractOcrService(string tessdataPath, IUnitOfWork unitOfWork, IS3Service s3Service)
		{
			_tessdataPath = tessdataPath;
			_unitOfWork = unitOfWork;
			_s3Service = s3Service;
		}

		public async Task<string> ExtractText(long examId, string imagePath, IFormFile file, string language = "eng")
		{
			// Đọc file ảnh vào memory
			byte[] imageBytes;

			using (var ms = new MemoryStream())
			{
				await file.CopyToAsync(ms);
				imageBytes = ms.ToArray();
			}

			// OCR bằng memory
			//using var pix = Pix.LoadFromMemory(imageBytes);
			//using var engine = new TesseractEngine(_tessdataPath, language, EngineMode.Default);
			//using var page = engine.Process(pix);

			//string text = ExtractProblemStatement(page.GetText());

			var exam = await _unitOfWork.ExamRepository.GetByIdAsync(examId);
			var s3Path = $"{exam.ExamCode}";
			string imageS3Url;

			using var uploadStream = new MemoryStream(imageBytes);
			imageS3Url = await _s3Service.UploadImageAsync(
				uploadStream,
				file.FileName,
				s3Path
			);
			exam.ExamPaper = imageS3Url;
			await _unitOfWork.SaveChangesAsync();
			return imageS3Url;
		}

		private string ExtractProblemStatement(string rawText)
		{
			if (string.IsNullOrWhiteSpace(rawText))
				return "";

			string final = rawText.Replace("\r", "");
			final = Regex.Replace(final, @"[ ]{2,}", " ");
			final = Regex.Replace(final, @"\n{2,}", "\n");

			// tìm "Problem statement"
			var problemMatch = Regex.Match(final, @"Problem\s*statement", RegexOptions.IgnoreCase);

			if (!problemMatch.Success)
				return "";

			string afterProblem = final.Substring(problemMatch.Index);

			// tìm Part X để giới hạn
			var partMatch = Regex.Match(afterProblem, @"\n\s*Part\s*\d+", RegexOptions.IgnoreCase);

			string problemContent;

			if (partMatch.Success)
			{
				// cắt tới trước Part 1
				problemContent = afterProblem.Substring(0, partMatch.Index).Trim();
			}
			else
			{
				// không có Part → lấy hết
				problemContent = afterProblem.Trim();
			}

			// xoá tiêu đề Problem statement
			problemContent = Regex.Replace(
				problemContent,
				@"^\s*Problem\s*statement[:\s]*",
				"",
				RegexOptions.IgnoreCase
			).Trim();
			// =============================
			// FINAL BEAUTY FORMATTING
			// =============================

			// Convert OCR bullet mistakes
			problemContent = Regex.Replace(problemContent, @"(^|\n)\s*eo\s+", "\n• ", RegexOptions.IgnoreCase);
			problemContent = Regex.Replace(problemContent, @"(^|\n)\s*e\s+", "\n• ", RegexOptions.IgnoreCase);

			// Fix wrapped lines inside parentheses or long sentences
			problemContent = Regex.Replace(problemContent, @"\n([a-z])", " $1");
			problemContent = Regex.Replace(problemContent, @"\n\(", " (");
			problemContent = Regex.Replace(problemContent, @"\n([A-Z][a-z]+)", " $1");

			// Ensure newline before first bullet list
			problemContent = Regex.Replace(problemContent, @"(:)\s*•", "$1\n•");

			// Ensure spacing between paragraphs (2 newlines)
			problemContent = Regex.Replace(problemContent, @"([a-z]\.)\s+([A-Z])", "$1\n\n$2");

			// Clean extra spaces / lines
			problemContent = Regex.Replace(problemContent, @"[ ]{2,}", " ");
			problemContent = Regex.Replace(problemContent, @"\n{3,}", "\n\n");

			// Trim final whitespace
			problemContent = problemContent.Trim();
			return problemContent;
		}
	}
}
