using BLL.Interface;
using BLL.Model.Response;
using DAL.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
		private readonly IAIVerificationService _aiVerificationService;
		private readonly ILogger<PlagiarismService> _logger;

		public PlagiarismService(
			IUnitOfWork unitOfWork, 
			IVectorService vectorService,
			IAIVerificationService aiVerificationService,
			ILogger<PlagiarismService> logger)
		{
			_unitOfWork = unitOfWork;
			_vectorService = vectorService;
			_aiVerificationService = aiVerificationService;
			_logger = logger;
		}

		public async Task<PlagiarismCheckResponse> CheckSuspiciousDocumentAsync(long docFileId, decimal threshold, int userId)
		{
			_logger.LogInformation($"[PlagiarismCheck] Starting plagiarism check for DocFile ID: {docFileId}, Threshold: {threshold:P0}, User: {userId}");

			// 1. Validate and get the suspicious document
			var docFile = await _unitOfWork.DocFileRepository.GetByIdAsync(docFileId);
			if (docFile == null)
			{
				_logger.LogWarning($"[PlagiarismCheck] DocFile {docFileId} not found");
				throw new ArgumentException($"DocFile with ID {docFileId} not found");
			}

			// 2. Get exam info through ExamStudent
			var examStudent = await _unitOfWork.ExamStudentRepository.GetByIdAsync(docFile.ExamStudentId);
			if (examStudent == null)
			{
				_logger.LogWarning($"[PlagiarismCheck] ExamStudent {docFile.ExamStudentId} not found for DocFile {docFileId}");
				throw new InvalidOperationException($"ExamStudent not found for DocFile {docFileId}");
			}

			var examId = examStudent.ExamId;
			var exam = await _unitOfWork.ExamRepository.GetByIdAsync(examId);
			if (exam == null)
			{
				_logger.LogWarning($"[PlagiarismCheck] Exam {examId} not found");
				throw new ArgumentException($"Exam with ID {examId} not found");
			}

			// 3. Get student info
			var student = await _unitOfWork.StudentRepository.GetByIdAsync(examStudent.StudentId);
			var studentCode = student?.StudentCode ?? "Unknown";

			_logger.LogInformation($"[PlagiarismCheck] Checking DocFile {docFileId} (Student: {studentCode}) in Exam {examId} ({exam.ExamCode})");

			// 4. Validate user exists
			var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
			if (user == null)
			{
				_logger.LogWarning($"[PlagiarismCheck] User {userId} not found");
				throw new ArgumentException($"User with ID {userId} not found");
			}

			// 5. Validate document has parsed text
			if (docFile.ParseStatus != DocParseStatus.OK || string.IsNullOrWhiteSpace(docFile.ParsedText))
			{
				_logger.LogWarning($"[PlagiarismCheck] DocFile {docFileId} has not been successfully parsed (Status: {docFile.ParseStatus})");
				throw new InvalidOperationException($"Document {docFileId} has not been successfully parsed or has no text content");
			}

			// 6. Ensure Qdrant collection exists
			await _vectorService.EnsureCollectionExistsAsync();

		// 7. Always re-index when manually calling plagiarism check API to ensure latest data
		_logger.LogInformation($"[PlagiarismCheck] Re-indexing DocFile {docFileId} (manual check always re-indexes)");
					await _vectorService.IndexDocumentAsync(
						docFileId: docFileId,
						examId: examId,
						studentCode: studentCode,
						text: docFile.ParsedText
					);
		
		// Mark as embedded
		docFile.IsEmbedded = true;
		await _unitOfWork.SaveChangesAsync();

			// 8. Search for similar documents using Qdrant's vector search
			_logger.LogInformation($"[PlagiarismCheck] Searching for similar documents in Exam {examId}...");
			var similarPairs = await _vectorService.SearchSimilarToDocumentAsync(docFileId, examId, (float)threshold);

			_logger.LogInformation($"[PlagiarismCheck] Found {similarPairs.Count} suspicious document(s) similar to DocFile {docFileId}");

			// 9. Create SimilarityCheck record
			var similarityCheck = new SimilarityCheck
			{
				ExamId = examId,
				CheckedAt = DateTime.UtcNow,
				Threshold = threshold,
				CheckedByUserId = userId
			};

			await _unitOfWork.SimilarityCheckRepository.AddAsync(similarityCheck);
			await _unitOfWork.SaveChangesAsync();

			_logger.LogInformation($"[PlagiarismCheck] Created SimilarityCheck record with ID: {similarityCheck.Id}");

			// 10. Create SimilarityResult records for each match
			var similarityResults = new List<SimilarityResult>();
			foreach (var pair in similarPairs)
			{
				// Get the matched document info
				var matchedDocFile = await _unitOfWork.DocFileRepository.GetByIdAsync(pair.DocFile2Id);
				if (matchedDocFile == null) continue;

				var matchedExamStudent = await _unitOfWork.ExamStudentRepository.GetByIdAsync(matchedDocFile.ExamStudentId);
				if (matchedExamStudent == null) continue;

				var matchedStudent = await _unitOfWork.StudentRepository.GetByIdAsync(matchedExamStudent.StudentId);
				var matchedStudentCode = matchedStudent?.StudentCode ?? "Unknown";

				var similarityResult = new SimilarityResult
				{
					SimilarityCheckId = similarityCheck.Id,
					DocFile1Id = pair.DocFile1Id,
					DocFile2Id = pair.DocFile2Id,
					SimilarityScore = (decimal)pair.SimilarityScore,
					Student1Code = studentCode,
					Student2Code = matchedStudentCode
				};

				await _unitOfWork.SimilarityCheckRepository.GetDbContext()
					.Set<SimilarityResult>().AddAsync(similarityResult);

				similarityResults.Add(similarityResult);

				_logger.LogInformation($"[PlagiarismCheck] Recorded match: {studentCode} <-> {matchedStudentCode} (Score: {pair.SimilarityScore:P2})");
			}

			await _unitOfWork.SaveChangesAsync();

			_logger.LogInformation($"[PlagiarismCheck] ✓ Plagiarism check completed successfully. Check ID: {similarityCheck.Id}");

		// 11. Build response with file paths
		var response = new PlagiarismCheckResponse
		{
			CheckId = similarityCheck.Id,
			ExamId = examId,
			ExamCode = exam.ExamCode,
			CheckedAt = similarityCheck.CheckedAt,
			Threshold = threshold,
			TotalPairsChecked = similarPairs.Count,
			SuspiciousPairsCount = similarPairs.Count,
			CheckedByUsername = user.Username,
			SuspiciousPairs = similarityResults.Select(result => 
			{
				var matchedDocFile = _unitOfWork.DocFileRepository.GetByIdAsync(result.DocFile2Id).Result;
				
				return new SimilarityPairResponse
				{
					ResultId = result.Id,
					Student1Code = result.Student1Code ?? "Unknown",
					Student2Code = result.Student2Code ?? "Unknown",
					DocFile1Name = docFile.FileName,
					DocFile2Name = matchedDocFile?.FileName,
					DocFile1Id = result.DocFile1Id,
					DocFile2Id = result.DocFile2Id,
					SimilarityScore = result.SimilarityScore,
					// Return file paths from DocFile records
					DocFile1Path = docFile.FilePath,
					DocFile2Path = matchedDocFile?.FilePath
				};
			}).ToList()
		};

		return response;
		}

		public async Task GenerateEmbeddingForDocFileAsync(long docFileId)
		{
			_logger.LogInformation($"[GenerateEmbedding] Starting embedding generation for DocFile {docFileId}");

			var docFile = await _unitOfWork.DocFileRepository.GetByIdAsync(docFileId);
			if (docFile == null)
			{
				_logger.LogWarning($"[GenerateEmbedding] DocFile {docFileId} not found");
				throw new ArgumentException($"DocFile with ID {docFileId} not found");
			}

			// Only generate embedding if document is successfully parsed
			if (docFile.ParseStatus != DocParseStatus.OK || string.IsNullOrWhiteSpace(docFile.ParsedText))
			{
				_logger.LogWarning($"[GenerateEmbedding] DocFile {docFileId} has not been parsed or has no text (Status: {docFile.ParseStatus})");
				return;
			}

		// Check if already embedded (for automatic background job processing)
		if (docFile.IsEmbedded)
			{
			_logger.LogInformation($"[GenerateEmbedding] DocFile {docFileId} is already embedded, skipping");
				return;
			}

			// Get exam and student info
			var examStudent = await _unitOfWork.ExamStudentRepository.GetByIdAsync(docFile.ExamStudentId);
			if (examStudent == null)
			{
				_logger.LogWarning($"[GenerateEmbedding] ExamStudent {docFile.ExamStudentId} not found for DocFile {docFileId}");
				throw new InvalidOperationException($"ExamStudent not found for DocFile {docFileId}");
			}

			var student = await _unitOfWork.StudentRepository.GetByIdAsync(examStudent.StudentId);
			var studentCode = student?.StudentCode ?? "Unknown";

			_logger.LogInformation($"[GenerateEmbedding] Indexing DocFile {docFileId} for Student {studentCode} in Exam {examStudent.ExamId}");

			// Ensure collection exists
			await _vectorService.EnsureCollectionExistsAsync();

			// Index the document
			await _vectorService.IndexDocumentAsync(
				docFileId: docFileId,
				examId: examStudent.ExamId,
				studentCode: studentCode,
				text: docFile.ParsedText
			);

		// Mark as embedded
		docFile.IsEmbedded = true;
		await _unitOfWork.SaveChangesAsync();

			_logger.LogInformation($"[GenerateEmbedding] ✓ Successfully generated and indexed embedding for DocFile {docFileId}");
		}

		public async Task<VerificationResponse> VerifyWithAIAsync(long similarityResultId)
		{
			_logger.LogInformation($"[AIVerify] Starting AI verification for SimilarityResult ID: {similarityResultId}");

			// Get the similarity result with related entities
			var dbContext = _unitOfWork.SimilarityCheckRepository.GetDbContext();
			var similarityResult = await dbContext.Set<SimilarityResult>()
				.Include(sr => sr.DocFile1)
				.Include(sr => sr.DocFile2)
				.FirstOrDefaultAsync(sr => sr.Id == similarityResultId);

			if (similarityResult == null)
			{
				_logger.LogWarning($"[AIVerify] SimilarityResult {similarityResultId} not found");
				throw new ArgumentException($"SimilarityResult with ID {similarityResultId} not found");
			}

			// Check if already verified by AI
			if (similarityResult.VerificationStatus != VerificationStatus.Pending)
			{
				_logger.LogWarning($"[AIVerify] SimilarityResult {similarityResultId} is already verified (Status: {similarityResult.VerificationStatus})");
				throw new InvalidOperationException($"This result has already been verified. Current status: {similarityResult.VerificationStatus}");
			}

			// Get text content from both documents
			var docFile1 = similarityResult.DocFile1;
			var docFile2 = similarityResult.DocFile2;

			if (string.IsNullOrWhiteSpace(docFile1.ParsedText) || string.IsNullOrWhiteSpace(docFile2.ParsedText))
			{
				_logger.LogWarning($"[AIVerify] One or both documents don't have parsed text");
				throw new InvalidOperationException("One or both documents don't have text content for verification");
			}

			_logger.LogInformation($"[AIVerify] Calling AI to verify {similarityResult.Student1Code} vs {similarityResult.Student2Code}");

			// Call AI verification service
			var aiResult = await _aiVerificationService.VerifyTextSimilarityAsync(
				docFile1.ParsedText,
				docFile2.ParsedText,
				similarityResult.Student1Code ?? "Unknown",
				similarityResult.Student2Code ?? "Unknown"
			);

			// Update similarity result
			similarityResult.VerificationStatus = aiResult.IsSimilar 
				? VerificationStatus.AIVerified_Similar 
				: VerificationStatus.AIVerified_NotSimilar;
			
			similarityResult.AIVerificationResult = System.Text.Json.JsonSerializer.Serialize(new
			{
				isSimilar = aiResult.IsSimilar,
				confidenceScore = aiResult.ConfidenceScore,
				summary = aiResult.Summary,
				analysis = aiResult.Analysis
			});
			
			similarityResult.AIVerifiedAt = DateTime.UtcNow;

			await _unitOfWork.SaveChangesAsync();

			_logger.LogInformation($"[AIVerify] ✓ AI verification completed: Similar={aiResult.IsSimilar}, Status={similarityResult.VerificationStatus}");

			return new VerificationResponse
			{
				SimilarityResultId = similarityResult.Id,
				Student1Code = similarityResult.Student1Code ?? "Unknown",
				Student2Code = similarityResult.Student2Code ?? "Unknown",
				SimilarityScore = similarityResult.SimilarityScore,
				VerificationStatus = similarityResult.VerificationStatus,
				VerificationStatusText = similarityResult.VerificationStatus.ToString(),
				AIVerifiedSimilar = aiResult.IsSimilar,
				AIConfidenceScore = aiResult.ConfidenceScore,
				AISummary = aiResult.Summary,
				AIAnalysis = aiResult.Analysis,
				AIVerifiedAt = similarityResult.AIVerifiedAt
			};
		}

		public async Task<VerificationResponse> TeacherVerifyAsync(long similarityResultId, bool isSimilar, string? notes, int userId)
		{
			_logger.LogInformation($"[TeacherVerify] Starting teacher verification for SimilarityResult ID: {similarityResultId} by User {userId}");

			// Get the similarity result
			var dbContext = _unitOfWork.SimilarityCheckRepository.GetDbContext();
			var similarityResult = await dbContext.Set<SimilarityResult>()
				.Include(sr => sr.TeacherVerifiedByUser)
				.FirstOrDefaultAsync(sr => sr.Id == similarityResultId);

			if (similarityResult == null)
			{
				_logger.LogWarning($"[TeacherVerify] SimilarityResult {similarityResultId} not found");
				throw new ArgumentException($"SimilarityResult with ID {similarityResultId} not found");
			}

			// Get teacher user
			var teacher = await _unitOfWork.UserRepository.GetByIdAsync(userId);
			if (teacher == null)
			{
				_logger.LogWarning($"[TeacherVerify] User {userId} not found");
				throw new ArgumentException($"User with ID {userId} not found");
			}

			// Update verification status
			similarityResult.VerificationStatus = isSimilar 
				? VerificationStatus.TeacherConfirmed_Similar 
				: VerificationStatus.TeacherConfirmed_NotSimilar;
			
			similarityResult.TeacherVerifiedByUserId = userId;
			similarityResult.TeacherVerifiedAt = DateTime.UtcNow;
			similarityResult.TeacherNotes = notes;

			await _unitOfWork.SaveChangesAsync();

			_logger.LogInformation($"[TeacherVerify] ✓ Teacher verification completed: Similar={isSimilar}, Teacher={teacher.Username}");

			// Parse AI verification result if exists
			bool? aiVerifiedSimilar = null;
			decimal? aiConfidenceScore = null;
			string? aiSummary = null;
			string? aiAnalysis = null;

			if (!string.IsNullOrWhiteSpace(similarityResult.AIVerificationResult))
			{
				try
				{
					var aiData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(similarityResult.AIVerificationResult);
					aiVerifiedSimilar = aiData.GetProperty("isSimilar").GetBoolean();
					aiConfidenceScore = aiData.GetProperty("confidenceScore").GetDecimal();
					aiSummary = aiData.GetProperty("summary").GetString();
					aiAnalysis = aiData.GetProperty("analysis").GetString();
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, $"[TeacherVerify] Failed to parse AI verification result");
				}
			}

			return new VerificationResponse
			{
				SimilarityResultId = similarityResult.Id,
				Student1Code = similarityResult.Student1Code ?? "Unknown",
				Student2Code = similarityResult.Student2Code ?? "Unknown",
				SimilarityScore = similarityResult.SimilarityScore,
				VerificationStatus = similarityResult.VerificationStatus,
				VerificationStatusText = similarityResult.VerificationStatus.ToString(),
				AIVerifiedSimilar = aiVerifiedSimilar,
				AIConfidenceScore = aiConfidenceScore,
				AISummary = aiSummary,
				AIAnalysis = aiAnalysis,
				AIVerifiedAt = similarityResult.AIVerifiedAt,
				TeacherVerifiedSimilar = isSimilar,
				TeacherUsername = teacher.Username,
				TeacherNotes = notes,
				TeacherVerifiedAt = similarityResult.TeacherVerifiedAt
			};
		}
	}

	// Extension method to get DbContext from repository
	internal static class RepositoryExtensions
	{
		public static DAL.SWDGradingDbContext GetDbContext(this ISimilarityCheckRepository repository)
		{
			// Access the internal context through reflection
			var field = repository.GetType().GetField("_context", 
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			return (DAL.SWDGradingDbContext)field!.GetValue(repository)!;
		}
	}
}
