using BLL.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
	public class VectorService : IVectorService
	{
		private readonly QdrantClient _qdrantClient;
		private readonly string _collectionName;
		private readonly uint _vectorSize;
		private readonly ILogger<VectorService> _logger;

	public VectorService(IConfiguration configuration, ILogger<VectorService> logger)
	{
		_logger = logger;
		var qdrantEndpoint = configuration["Qdrant:Endpoint"] ?? "http://localhost:6333";
		_collectionName = configuration["Qdrant:CollectionName"] ?? "exam_submissions";
		_vectorSize = uint.Parse(configuration["Qdrant:VectorSize"] ?? "384");

		// Parse the endpoint URL to extract host and port
		var uri = new Uri(qdrantEndpoint);
		var host = uri.Host;
		var port = uri.Port;
		var useHttps = uri.Scheme == "https";

		_qdrantClient = new QdrantClient(host: host, port: port, https: useHttps);
	}

		public async Task EnsureCollectionExistsAsync()
		{
			try
			{
				// Check if collection exists
				var collections = await _qdrantClient.ListCollectionsAsync();
				var collectionExists = collections.Contains(_collectionName);

				if (!collectionExists)
				{
					_logger.LogInformation($"Creating Qdrant collection: {_collectionName}");
					// Create collection with cosine distance
					await _qdrantClient.CreateCollectionAsync(
						collectionName: _collectionName,
						vectorsConfig: new VectorParams
						{
							Size = _vectorSize,
							Distance = Distance.Cosine
						}
					);
					_logger.LogInformation($"Collection {_collectionName} created successfully");
				}
				else
				{
					_logger.LogInformation($"Collection {_collectionName} already exists");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to ensure Qdrant collection exists");
				throw;
			}
		}

		public async Task<float[]> GenerateEmbeddingAsync(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return new float[_vectorSize];
			}

			// Simplified embedding generation using text hashing
			// In production, use actual ONNX model inference
			return await Task.Run(() => GenerateSimpleEmbedding(text));
		}

		private float[] GenerateSimpleEmbedding(string text)
		{
			// Normalize text
			text = text.ToLowerInvariant();
			var words = text.Split(new[] { ' ', '\n', '\r', '\t', '.', ',', ';', ':', '!', '?' }, 
				StringSplitOptions.RemoveEmptyEntries);

			// Create embedding vector
			var embedding = new float[_vectorSize];

			// Use word frequencies and positions to create features
			var wordFreq = new Dictionary<string, int>();
			foreach (var word in words)
			{
				if (word.Length > 2) // Skip very short words
				{
					wordFreq[word] = wordFreq.GetValueOrDefault(word, 0) + 1;
				}
			}

			// Generate embedding based on text characteristics
			foreach (var kvp in wordFreq)
			{
				var wordHash = GetStableHashCode(kvp.Key);
				for (int i = 0; i < _vectorSize; i++)
				{
					var seed = wordHash + i;
					var rand = new Random(seed);
					embedding[i] += (float)(rand.NextDouble() * 2 - 1) * kvp.Value;
				}
			}

			// Normalize vector
			var magnitude = Math.Sqrt(embedding.Sum(x => x * x));
			if (magnitude > 0)
			{
				for (int i = 0; i < embedding.Length; i++)
				{
					embedding[i] /= (float)magnitude;
				}
			}

			return embedding;
		}

		private int GetStableHashCode(string str)
		{
			unchecked
			{
				int hash1 = 5381;
				int hash2 = hash1;

				for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
				{
					hash1 = ((hash1 << 5) + hash1) ^ str[i];
					if (i == str.Length - 1 || str[i + 1] == '\0')
						break;
					hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
				}

				return hash1 + (hash2 * 1566083941);
			}
		}

		public async Task IndexDocumentAsync(long docFileId, long examId, string studentCode, string text)
		{
			try
			{
				var embedding = await GenerateEmbeddingAsync(text);

				var point = new PointStruct
				{
					Id = new PointId { Num = (ulong)docFileId },
					Vectors = embedding,
					Payload =
					{
						["docFileId"] = docFileId,
						["examId"] = examId,
						["studentCode"] = studentCode,
						["textLength"] = text.Length
					}
				};

				await _qdrantClient.UpsertAsync(
					collectionName: _collectionName,
					points: new[] { point }
				);

				_logger.LogInformation($"Indexed document {docFileId} for exam {examId}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Failed to index document {docFileId}");
				throw;
			}
		}

		public async Task<bool> IsDocumentIndexedAsync(long docFileId)
		{
			try
			{
				var points = await _qdrantClient.RetrieveAsync(
					collectionName: _collectionName,
					ids: new[] { new PointId { Num = (ulong)docFileId } },
					withPayload: false,
					withVectors: false
				);

				return points.Any();
			}
			catch
			{
				return false;
			}
		}

		public async Task<List<SimilarityPair>> SearchSimilarDocumentsAsync(long examId, float threshold)
		{
			var similarPairs = new List<SimilarityPair>();

			try
			{
				// First, get all documents for this exam using Scroll
				var filter = new Filter
				{
					Must =
					{
						new Condition
						{
							Field = new FieldCondition
							{
								Key = "examId",
								Match = new Match { Integer = examId }
							}
						}
					}
				};

			var scrollResponse = await _qdrantClient.ScrollAsync(
				collectionName: _collectionName,
				filter: filter,
				limit: 1000,
				payloadSelector: true,
				vectorsSelector: true
			);
			
			var documents = scrollResponse.Result.ToList();

			_logger.LogInformation($"Found {documents.Count} documents for exam {examId}");

			// Compare each pair of documents
			for (int i = 0; i < documents.Count; i++)
			{
				for (int j = i + 1; j < documents.Count; j++)
					{
						var doc1 = documents[i];
						var doc2 = documents[j];

						var vector1 = doc1.Vectors.Vector.Data.ToArray();
						var vector2 = doc2.Vectors.Vector.Data.ToArray();

						var similarity = CosineSimilarity(vector1, vector2);

						if (similarity >= threshold)
						{
							similarPairs.Add(new SimilarityPair
							{
								DocFile1Id = (long)doc1.Payload["docFileId"].IntegerValue,
								DocFile2Id = (long)doc2.Payload["docFileId"].IntegerValue,
								SimilarityScore = similarity
							});
						}
					}
				}

				_logger.LogInformation($"Found {similarPairs.Count} similar pairs with threshold {threshold}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to search similar documents");
				throw;
			}

			return similarPairs;
		}

		private float CosineSimilarity(float[] vector1, float[] vector2)
		{
			if (vector1.Length != vector2.Length)
				throw new ArgumentException("Vectors must have the same length");

			float dotProduct = 0;
			float magnitude1 = 0;
			float magnitude2 = 0;

			for (int i = 0; i < vector1.Length; i++)
			{
				dotProduct += vector1[i] * vector2[i];
				magnitude1 += vector1[i] * vector1[i];
				magnitude2 += vector2[i] * vector2[i];
			}

			magnitude1 = (float)Math.Sqrt(magnitude1);
			magnitude2 = (float)Math.Sqrt(magnitude2);

			if (magnitude1 == 0 || magnitude2 == 0)
				return 0;

			return dotProduct / (magnitude1 * magnitude2);
		}
	}
}
