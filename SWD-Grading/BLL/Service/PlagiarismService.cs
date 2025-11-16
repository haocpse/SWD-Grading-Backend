using BLL.Interface;
using BLL.Model.Response;
using DAL.Interface;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
	public class PlagiarismService : IPlagiarismService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IVectorService _vectorService;

		public PlagiarismService(IUnitOfWork unitOfWork, IVectorService vectorService)
		{
			_unitOfWork = unitOfWork;
			_vectorService = vectorService;
		}

		public async Task<PlagiarismCheckResponse> CheckPlagiarismAsync(long examId, decimal threshold, int userId)
		{
			// Validate exam exists
			var exam = await _unitOfWork.ExamRepository.GetByIdAsync(examId);
			if (exam == null)
			{
				throw new ArgumentException($"Exam with ID {examId} not found");
			}

			// Validate user exists
			var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
			if (user == null)
			{
				throw new ArgumentException($"User with ID {userId} not found");
			}

			// Ensure Qdrant collection exists
			await _vectorService.EnsureCollectionExistsAsync();

			// Get all DocFiles for this exam with parsed text
			var docFiles = await GetDocFilesForExamAsync(examId);

			if (docFiles.Count < 2)
			{
				throw new InvalidOperationException("Need at least 2 documents to check for plagiarism");
			}

			// Index documents that haven't been indexed yet
			await IndexMissingDocumentsAsync(docFiles);

			// Search for similar documents
			var similarPairs = await _vectorService.SearchSimilarDocumentsAsync(examId, (float)threshold);

			// Create SimilarityCheck record
			var similarityCheck = new SimilarityCheck
			{
				ExamId = examId,
				CheckedAt = DateTime.UtcNow,
				Threshold = threshold,
				CheckedByUserId = userId
			};

			await _unitOfWork.SimilarityCheckRepository.AddAsync(similarityCheck);
			await _unitOfWork.SaveChangesAsync();

			// Create SimilarityResult records
			var docFileDict = docFiles.ToDictionary(df => df.Id);
			foreach (var pair in similarPairs)
			{
				var docFile1 = docFileDict[pair.DocFile1Id];
				var docFile2 = docFileDict[pair.DocFile2Id];

				var similarityResult = new SimilarityResult
				{
					SimilarityCheckId = similarityCheck.Id,
					DocFile1Id = pair.DocFile1Id,
					DocFile2Id = pair.DocFile2Id,
					SimilarityScore = (decimal)pair.SimilarityScore,
					Student1Code = docFile1.ExamStudent?.Student?.StudentCode,
					Student2Code = docFile2.ExamStudent?.Student?.StudentCode
				};

				await _unitOfWork.SimilarityCheckRepository.GetDbContext()
					.Set<SimilarityResult>().AddAsync(similarityResult);
			}

			await _unitOfWork.SaveChangesAsync();

			// Build response
			var response = new PlagiarismCheckResponse
			{
				CheckId = similarityCheck.Id,
				ExamId = examId,
				ExamCode = exam.ExamCode,
				CheckedAt = similarityCheck.CheckedAt,
				Threshold = threshold,
				TotalPairsChecked = CalculateTotalPairs(docFiles.Count),
				SuspiciousPairsCount = similarPairs.Count,
				CheckedByUsername = user.Username,
				SuspiciousPairs = similarPairs.Select(pair =>
				{
					var docFile1 = docFileDict[pair.DocFile1Id];
					var docFile2 = docFileDict[pair.DocFile2Id];

					return new SimilarityPairResponse
					{
						ResultId = 0, // Will be set after save
						Student1Code = docFile1.ExamStudent?.Student?.StudentCode ?? "Unknown",
						Student2Code = docFile2.ExamStudent?.Student?.StudentCode ?? "Unknown",
						DocFile1Name = docFile1.FileName,
						DocFile2Name = docFile2.FileName,
						DocFile1Id = pair.DocFile1Id,
						DocFile2Id = pair.DocFile2Id,
						SimilarityScore = (decimal)pair.SimilarityScore
					};
				}).ToList()
			};

			return response;
		}

		public async Task<List<PlagiarismCheckResponse>> GetCheckHistoryAsync(long examId)
		{
			var checks = await _unitOfWork.SimilarityCheckRepository.GetCheckHistoryByExamIdAsync(examId);

			return checks.Select(check => new PlagiarismCheckResponse
			{
				CheckId = check.Id,
				ExamId = check.ExamId,
				ExamCode = check.Exam?.ExamCode,
				CheckedAt = check.CheckedAt,
				Threshold = check.Threshold,
				TotalPairsChecked = 0, // Would need to calculate from exam
				SuspiciousPairsCount = check.SimilarityResults?.Count ?? 0,
				CheckedByUsername = check.CheckedByUser?.Username ?? "Unknown",
				SuspiciousPairs = check.SimilarityResults?.Select(result => new SimilarityPairResponse
				{
					ResultId = result.Id,
					Student1Code = result.Student1Code ?? "Unknown",
					Student2Code = result.Student2Code ?? "Unknown",
					DocFile1Name = result.DocFile1?.FileName,
					DocFile2Name = result.DocFile2?.FileName,
					DocFile1Id = result.DocFile1Id,
					DocFile2Id = result.DocFile2Id,
					SimilarityScore = result.SimilarityScore
				}).ToList() ?? new List<SimilarityPairResponse>()
			}).ToList();
		}

		public async Task GenerateEmbeddingForDocFileAsync(long docFileId)
		{
			var docFile = await _unitOfWork.DocFileRepository.GetByIdAsync(docFileId);
			if (docFile == null)
			{
				throw new ArgumentException($"DocFile with ID {docFileId} not found");
			}

			// Only generate embedding if document is successfully parsed
			if (docFile.ParseStatus != DocParseStatus.OK || string.IsNullOrWhiteSpace(docFile.ParsedText))
			{
				return;
			}

			// Check if already indexed
			var isIndexed = await _vectorService.IsDocumentIndexedAsync(docFileId);
			if (isIndexed)
			{
				return;
			}

			// Get exam and student info
			var examStudent = await _unitOfWork.ExamStudentRepository.GetByIdAsync(docFile.ExamStudentId);
			if (examStudent == null)
			{
				throw new InvalidOperationException($"ExamStudent not found for DocFile {docFileId}");
			}

			var student = await _unitOfWork.StudentRepository.GetByIdAsync(examStudent.StudentId);
			var studentCode = student?.StudentCode ?? "Unknown";

			// Ensure collection exists
			await _vectorService.EnsureCollectionExistsAsync();

			// Index the document
			await _vectorService.IndexDocumentAsync(
				docFileId: docFileId,
				examId: examStudent.ExamId,
				studentCode: studentCode,
				text: docFile.ParsedText
			);
		}

		private async Task<List<DocFile>> GetDocFilesForExamAsync(long examId)
		{
			// Get all exam students for this exam
			var examStudents = await _unitOfWork.ExamStudentRepository.GetByExamIdWithDetailsAsync(examId, 0, int.MaxValue);
			var examStudentIds = examStudents.Select(es => es.Id).ToList();

			// Get all doc files for these exam students with ParseStatus = OK
			var allDocFiles = new List<DocFile>();
			foreach (var examStudentId in examStudentIds)
			{
				var docFiles = await _unitOfWork.DocFileRepository.GetByExamStudentIdAsync(examStudentId);
				allDocFiles.AddRange(docFiles.Where(df => 
					df.ParseStatus == DocParseStatus.OK && 
					!string.IsNullOrWhiteSpace(df.ParsedText)));
			}

			return allDocFiles;
		}

		private async Task IndexMissingDocumentsAsync(List<DocFile> docFiles)
		{
			foreach (var docFile in docFiles)
			{
				var isIndexed = await _vectorService.IsDocumentIndexedAsync(docFile.Id);
				if (!isIndexed)
				{
					var examStudent = await _unitOfWork.ExamStudentRepository.GetByIdAsync(docFile.ExamStudentId);
					if (examStudent == null) continue;
					
					var student = await _unitOfWork.StudentRepository.GetByIdAsync(examStudent.StudentId);
					var studentCode = student?.StudentCode ?? "Unknown";

					await _vectorService.IndexDocumentAsync(
						docFileId: docFile.Id,
						examId: examStudent.ExamId,
						studentCode: studentCode,
						text: docFile.ParsedText!
					);
				}
			}
		}

		private int CalculateTotalPairs(int documentCount)
		{
			// Formula: n * (n - 1) / 2
			return documentCount * (documentCount - 1) / 2;
		}
	}

	// Extension method to get DbContext from repository
	internal static class RepositoryExtensions
	{
		public static DAL.SWDGradingDbContext GetDbContext(this ISimilarityCheckRepository repository)
		{
			// Access the internal context through reflection or provide a method in the repository
			// For now, we'll use a simpler approach
			var field = repository.GetType().GetField("_context", 
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			return (DAL.SWDGradingDbContext)field!.GetValue(repository)!;
		}
	}
}

