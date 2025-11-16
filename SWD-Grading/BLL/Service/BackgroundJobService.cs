using BLL.Interface;
using DAL.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Service
{
	public class BackgroundJobService : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<BackgroundJobService> _logger;
		private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);

		public BackgroundJobService(IServiceProvider serviceProvider, ILogger<BackgroundJobService> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Background Job Service started");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await ProcessPendingExamZipsAsync();
					await ProcessPendingEmbeddingsAsync();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error occurred while processing background jobs");
				}

				// Wait before next poll
				await Task.Delay(_pollInterval, stoppingToken);
			}

			_logger.LogInformation("Background Job Service stopped");
		}

		private async Task ProcessPendingExamZipsAsync()
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
				var fileProcessingService = scope.ServiceProvider.GetRequiredService<IFileProcessingService>();

				// Get all pending ExamZips
				var pendingExamZips = await unitOfWork.ExamZipRepository.GetPendingExamZipsAsync();

				if (pendingExamZips.Any())
				{
					_logger.LogInformation($"Found {pendingExamZips.Count} pending exam zip(s) to process");

					foreach (var examZip in pendingExamZips)
					{
						try
						{
							_logger.LogInformation($"Processing ExamZip ID: {examZip.Id}");
							await fileProcessingService.ProcessStudentSolutionsAsync(examZip.Id);
							_logger.LogInformation($"Successfully processed ExamZip ID: {examZip.Id}");
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, $"Error processing ExamZip ID: {examZip.Id}");
						}
					}
				}
			}
		}

		private async Task ProcessPendingEmbeddingsAsync()
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
				var plagiarismService = scope.ServiceProvider.GetRequiredService<IPlagiarismService>();
			
				// Get all DocFiles with ParseStatus = OK that might need embeddings
				// We'll process documents that were recently parsed
				var recentDocFiles = await unitOfWork.DocFileRepository.GetRecentlyParsedDocFilesAsync(limit: 10);
			
				if (recentDocFiles.Any())
				{
					_logger.LogInformation($"Found {recentDocFiles.Count} document(s) to generate embeddings");
			
					foreach (var docFile in recentDocFiles)
					{
						try
						{
							_logger.LogInformation($"Generating embedding for DocFile ID: {docFile.Id}");
							await plagiarismService.GenerateEmbeddingForDocFileAsync(docFile.Id);
							_logger.LogInformation($"Successfully generated embedding for DocFile ID: {docFile.Id}");
						}
						catch (Exception ex)
						{
							_logger.LogError(ex, $"Error generating embedding for DocFile ID: {docFile.Id}");
						}
					}
				}
			}
		}
	}
}

