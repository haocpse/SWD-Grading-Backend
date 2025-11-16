using BLL.Interface;
using DAL.Interface;
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

		public TesseractOcrService(string tessdataPath, IUnitOfWork unitOfWork)
		{
			_tessdataPath = tessdataPath;
			_unitOfWork = unitOfWork;
		}

		public async Task<string> ExtractText(long examId, string imagePath, string language = "eng")
		{
			using var engine = new TesseractEngine(_tessdataPath, language, EngineMode.Default);
			using var img = Pix.LoadFromFile(imagePath);
			using var page = engine.Process(img);
			string text = ExtractProblemStatement(page.GetText());
			var exam = await _unitOfWork.ExamRepository.GetByIdAsync(examId);
			exam.Description = text;
			await _unitOfWork.SaveChangesAsync();
			return text;
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
