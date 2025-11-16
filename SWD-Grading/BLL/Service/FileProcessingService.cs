using BLL.Interface;
using DAL.Interface;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Model.Entity;
using Model.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BLL.Service
{
	public class FileProcessingService : IFileProcessingService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IS3Service _s3Service;

		public FileProcessingService(IUnitOfWork unitOfWork, IS3Service s3Service)
		{
			_unitOfWork = unitOfWork;
			_s3Service = s3Service;
		}

		public async Task ProcessStudentSolutionsAsync(long examZipId)
		{
			var processSummary = new StringBuilder();
			int processedCount = 0;
			int successCount = 0;
			int errorCount = 0;
			var errors = new List<string>();

			try
			{
				// Get ExamZip record
				var examZip = await _unitOfWork.ExamZipRepository.GetByIdAsync(examZipId);
				if (examZip == null)
				{
					throw new Exception($"ExamZip with ID {examZipId} not found");
				}

				// Get Exam info
				var exam = await _unitOfWork.ExamRepository.GetByIdAsync(examZip.ExamId);
				if (exam == null)
				{
					throw new Exception($"Exam with ID {examZip.ExamId} not found");
				}

				// Check if ZIP file exists
				if (string.IsNullOrEmpty(examZip.ZipPath) || !File.Exists(examZip.ZipPath))
				{
					examZip.ParseStatus = ParseStatus.ERROR;
					examZip.ParseSummary = "ZIP file not found at specified path";
					await _unitOfWork.SaveChangesAsync();
					return;
				}

				// Create temp extraction directory
				var tempExtractPath = Path.Combine(Path.GetTempPath(), $"exam_{examZipId}_{Guid.NewGuid()}");
				Directory.CreateDirectory(tempExtractPath);

				try
				{
					// Extract main ZIP file
					ZipFile.ExtractToDirectory(examZip.ZipPath, tempExtractPath);
					examZip.ExtractedPath = tempExtractPath;

					// Get all student folders
					var studentFolders = Directory.GetDirectories(tempExtractPath);
					processSummary.AppendLine($"Found {studentFolders.Length} student folders");

					foreach (var studentFolder in studentFolders)
					{
						processedCount++;
						var studentFolderName = Path.GetFileName(studentFolder);

						try
						{
							await ProcessStudentFolderAsync(studentFolder, studentFolderName, examZip, exam);
							successCount++;
						}
						catch (Exception ex)
						{
							errorCount++;
							var errorMsg = $"Error processing {studentFolderName}: {ex.Message}";
							errors.Add(errorMsg);
							processSummary.AppendLine(errorMsg);
						}
					}

					// Update ExamZip status
					examZip.ParseStatus = errorCount == processedCount ? ParseStatus.ERROR : ParseStatus.DONE;
					processSummary.AppendLine($"\nProcessing complete:");
					processSummary.AppendLine($"Total: {processedCount}");
					processSummary.AppendLine($"Success: {successCount}");
					processSummary.AppendLine($"Errors: {errorCount}");
					examZip.ParseSummary = processSummary.ToString();

					await _unitOfWork.SaveChangesAsync();
				}
				finally
				{
					// Cleanup temp directory
					if (Directory.Exists(tempExtractPath))
					{
						try
						{
							Directory.Delete(tempExtractPath, true);
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error cleaning up temp directory: {ex.Message}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				// Update ExamZip with error status
				var examZip = await _unitOfWork.ExamZipRepository.GetByIdAsync(examZipId);
				if (examZip != null)
				{
					examZip.ParseStatus = ParseStatus.ERROR;
					examZip.ParseSummary = $"Fatal error: {ex.Message}\n{ex.StackTrace}";
					await _unitOfWork.SaveChangesAsync();
				}
				throw;
			}
		}

		private async Task ProcessStudentFolderAsync(string studentFolderPath, string folderName, ExamZip examZip, Exam exam)
		{
			// Parse student code from folder name
			// Format: Anhddhse170283 -> extract "se170283" or use whole name
			var studentCode = ExtractStudentCode(folderName);

			// Check if student exists, if not create
			var student = await _unitOfWork.StudentRepository.GetByStudentCodeAsync(studentCode);
			if (student == null)
			{
				student = new Student
				{
					StudentCode = studentCode,
					FullName = folderName, // Use folder name as full name initially
					Email = null,
					ClassName = null
				};
				await _unitOfWork.StudentRepository.AddAsync(student);
				await _unitOfWork.SaveChangesAsync();
			}

			// Create or get ExamStudent record
			var examStudent = await _unitOfWork.ExamStudentRepository.GetByExamAndStudentAsync(exam.Id, student.Id);
			if (examStudent == null)
			{
				examStudent = new ExamStudent
				{
					ExamId = exam.Id,
					StudentId = student.Id,
					Status = ExamStudentStatus.NOT_FOUND,
					Note = null
				};
				await _unitOfWork.ExamStudentRepository.AddAsync(examStudent);
				await _unitOfWork.SaveChangesAsync();
			}

			// Look for folder "0" inside student folder
			var zeroFolderPath = Path.Combine(studentFolderPath, "0");
			if (!Directory.Exists(zeroFolderPath))
			{
				examStudent.Status = ExamStudentStatus.NOT_FOUND;
				examStudent.Note = "Folder '0' not found";
				await _unitOfWork.SaveChangesAsync();
				return;
			}

			// Look for solution.zip inside "0" folder
			var solutionZipPath = Path.Combine(zeroFolderPath, "solution.zip");
			if (!File.Exists(solutionZipPath))
			{
				examStudent.Status = ExamStudentStatus.NOT_FOUND;
				examStudent.Note = "solution.zip not found in folder '0'";
				await _unitOfWork.SaveChangesAsync();
				return;
			}

			// Upload solution.zip to S3
			var s3Path = $"{exam.ExamCode}/{studentCode}";
			string solutionZipS3Url;
			using (var zipFileStream = File.OpenRead(solutionZipPath))
			{
				solutionZipS3Url = await _s3Service.UploadFileAsync(zipFileStream, "solution.zip", s3Path);
			}

			// Extract solution.zip to temp folder
			var tempSolutionExtractPath = Path.Combine(Path.GetTempPath(), $"solution_{Guid.NewGuid()}");
			Directory.CreateDirectory(tempSolutionExtractPath);

			try
			{
				ZipFile.ExtractToDirectory(solutionZipPath, tempSolutionExtractPath);

				// Find all .docx files
				var wordFiles = Directory.GetFiles(tempSolutionExtractPath, "*.docx", SearchOption.AllDirectories)
					.Where(f => !Path.GetFileName(f).StartsWith("~$")) // Exclude temp Word files
					.ToList();

				if (wordFiles.Count == 0)
				{
					// No Word files found
					examStudent.Status = ExamStudentStatus.NOT_FOUND;
					examStudent.Note = "No .docx files found in solution.zip";

					// Still create DocFile record with NOT_FOUND status
					var docFileNotFound = new DocFile
					{
						ExamStudentId = examStudent.Id,
						ExamZipId = examZip.Id,
						FileName = "solution.zip",
						FilePath = solutionZipS3Url,
						ParsedText = null,
						ParseStatus = DocParseStatus.NOT_FOUND,
						ParseMessage = "No Word document found in ZIP"
					};
					await _unitOfWork.DocFileRepository.AddAsync(docFileNotFound);
				}
				else
				{
					// Process each Word file
					foreach (var wordFilePath in wordFiles)
					{
						var fileName = Path.GetFileName(wordFilePath);

						// Upload Word file to S3
						string wordFileS3Url;
						using (var wordFileStream = File.OpenRead(wordFilePath))
						{
							wordFileS3Url = await _s3Service.UploadFileAsync(wordFileStream, fileName, s3Path);
						}

						// Extract text from Word document
						string extractedText = null;
						string parseMessage = null;
						DocParseStatus parseStatus;

						try
						{
							extractedText = ExtractTextFromWord(wordFilePath);
							parseStatus = DocParseStatus.OK;
							parseMessage = "Successfully parsed";
						}
						catch (Exception ex)
						{
							parseStatus = DocParseStatus.ERROR;
							parseMessage = $"Error parsing Word document: {ex.Message}";
						}

						// Create DocFile record
						var docFile = new DocFile
						{
							ExamStudentId = examStudent.Id,
							ExamZipId = examZip.Id,
							FileName = fileName,
							FilePath = wordFileS3Url,
							ParsedText = extractedText,
							ParseStatus = parseStatus,
							ParseMessage = parseMessage
						};
						await _unitOfWork.DocFileRepository.AddAsync(docFile);
					}

					// Update ExamStudent status
					examStudent.Status = ExamStudentStatus.PARSED;
					examStudent.Note = $"Processed {wordFiles.Count} Word file(s)";
				}

				await _unitOfWork.SaveChangesAsync();
			}
			finally
			{
				// Cleanup temp solution extraction folder
				if (Directory.Exists(tempSolutionExtractPath))
				{
					try
					{
						Directory.Delete(tempSolutionExtractPath, true);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error cleaning up solution temp directory: {ex.Message}");
					}
				}
			}
		}

		private string ExtractStudentCode(string folderName)
		{
			// Try to extract student code from folder name
			// Format: Anhddhse170283 -> extract "se170283"
			// Pattern: find last occurrence of "se" or "SE" followed by digits
			var match = Regex.Match(folderName, @"(se|SE)(\d+)", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				return match.Value.ToLower(); // Return "se170283"
			}

			// If no pattern found, use the entire folder name
			return folderName;
		}

		public string ExtractTextFromWord(string wordFilePath)
		{
			try
			{
				var text = new StringBuilder();

				using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(wordFilePath, false))
				{
					var body = wordDoc.MainDocumentPart?.Document?.Body;
					if (body == null)
					{
						return string.Empty;
					}

					// Extract all text from paragraphs
					foreach (var paragraph in body.Descendants<Paragraph>())
					{
						var paragraphText = paragraph.InnerText;
						if (!string.IsNullOrWhiteSpace(paragraphText))
						{
							text.AppendLine(paragraphText);
						}
					}

					// Extract text from tables
					foreach (var table in body.Descendants<Table>())
					{
						foreach (var row in table.Descendants<TableRow>())
						{
							var rowText = new List<string>();
							foreach (var cell in row.Descendants<TableCell>())
							{
								rowText.Add(cell.InnerText);
							}
							text.AppendLine(string.Join("\t", rowText));
						}
					}
				}

				return text.ToString();
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to extract text from Word document: {ex.Message}", ex);
			}
		}
	}
}

