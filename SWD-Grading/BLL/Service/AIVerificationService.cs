using BLL.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLL.Service
{
	public class AIVerificationService : IAIVerificationService
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private readonly ILogger<AIVerificationService> _logger;
		private readonly string _openAIApiKey;
		private readonly string _openAIModel;

		public AIVerificationService(IConfiguration configuration, ILogger<AIVerificationService> logger)
		{
			_logger = logger;
			_openAIApiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");
			_openAIModel = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
		}

		public async Task<AIVerificationResult> VerifyTextSimilarityAsync(string text1, string text2, string student1Code, string student2Code)
		{
			_logger.LogInformation($"[AIVerification] Starting AI verification for {student1Code} vs {student2Code}");

			try
			{
				var prompt = BuildVerificationPrompt(text1, text2, student1Code, student2Code);

				var requestBody = new
				{
					model = _openAIModel,
					messages = new[]
					{
						new { role = "system", content = "You are an expert academic integrity reviewer specializing in software design assignments. Analyze submissions to detect plagiarism while accounting for common technical terminology and standard software design patterns." },
						new { role = "user", content = prompt }
					},
					temperature = 0.3,
					max_tokens = 1000
				};

				var jsonContent = JsonSerializer.Serialize(requestBody);
				var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

				_httpClient.DefaultRequestHeaders.Clear();
				_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAIApiKey}");

				_logger.LogInformation($"[AIVerification] Calling OpenAI API with model {_openAIModel}...");

				var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
				response.EnsureSuccessStatusCode();

				var responseJson = await response.Content.ReadAsStringAsync();
				var jsonDoc = JsonDocument.Parse(responseJson);

				var aiResponse = jsonDoc.RootElement
					.GetProperty("choices")[0]
					.GetProperty("message")
					.GetProperty("content")
					.GetString() ?? string.Empty;

				_logger.LogInformation($"[AIVerification] Received AI response ({aiResponse.Length} chars)");

				// Parse AI response
				var result = ParseAIResponse(aiResponse);

				_logger.LogInformation($"[AIVerification] ✓ Verification complete: Similar={result.IsSimilar}, Confidence={result.ConfidenceScore:P0}");

				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"[AIVerification] ✗ Failed to verify similarity using AI");
				throw new InvalidOperationException("Failed to verify similarity using AI. Please try again later.", ex);
			}
		}

		private string BuildVerificationPrompt(string text1, string text2, string student1Code, string student2Code)
		{
			// Truncate texts if they're too long (keep first 3000 chars of each)
			var truncatedText1 = text1.Length > 3000 ? text1.Substring(0, 3000) + "..." : text1;
			var truncatedText2 = text2.Length > 3000 ? text2.Substring(0, 3000) + "..." : text2;

			return $@"You are reviewing two software design assignment submissions for potential plagiarism.

**Task**: Compare the two submissions and determine if they represent plagiarism or legitimate independent work.

**Student 1 ({student1Code}) Submission:**
{truncatedText1}

**Student 2 ({student2Code}) Submission:**
{truncatedText2}

**Evaluation Criteria:**
1. Structural similarity (document organization, section ordering)
2. Content similarity (ideas, concepts, explanations)
3. Phrasing and word choice
4. Diagrams/models descriptions (if any)
5. Examples and use cases

**Important Notes:**
- Common software design terminology (e.g., ""use case"", ""class diagram"", ""MVC pattern"") is expected and NOT plagiarism
- Standard software design patterns and best practices are common knowledge
- Focus on unique explanations, specific examples, and overall document structure

**Response Format (STRICTLY follow this JSON format):**
{{
  ""is_similar"": true/false,
  ""confidence_score"": 0.0-1.0,
  ""summary"": ""One sentence conclusion"",
  ""analysis"": ""Detailed analysis explaining your decision, including specific examples of similarities or differences. Maximum 500 words.""
}}

Provide your response as valid JSON only, no additional text.";
		}

		private AIVerificationResult ParseAIResponse(string aiResponse)
		{
			try
			{
				// Try to extract JSON from the response (in case AI adds extra text)
				var jsonStart = aiResponse.IndexOf('{');
				var jsonEnd = aiResponse.LastIndexOf('}');

				if (jsonStart >= 0 && jsonEnd > jsonStart)
				{
					var jsonStr = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
					var jsonDoc = JsonDocument.Parse(jsonStr);

					var isSimilar = jsonDoc.RootElement.GetProperty("is_similar").GetBoolean();
					var confidenceScore = jsonDoc.RootElement.GetProperty("confidence_score").GetDecimal();
					var summary = jsonDoc.RootElement.GetProperty("summary").GetString() ?? "";
					var analysis = jsonDoc.RootElement.GetProperty("analysis").GetString() ?? "";

					return new AIVerificationResult
					{
						IsSimilar = isSimilar,
						ConfidenceScore = confidenceScore,
						Summary = summary,
						Analysis = analysis
					};
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, $"[AIVerification] Failed to parse JSON response, using fallback parsing");
			}

			// Fallback: analyze the text content
			var responseLower = aiResponse.ToLower();
			var isSimilarFallback = responseLower.Contains("plagiarism") || 
			                        responseLower.Contains("is_similar\": true") ||
			                        responseLower.Contains("copied");

			return new AIVerificationResult
			{
				IsSimilar = isSimilarFallback,
				ConfidenceScore = 0.5m,
				Summary = "AI response could not be parsed properly",
				Analysis = aiResponse
			};
		}
	}
}

