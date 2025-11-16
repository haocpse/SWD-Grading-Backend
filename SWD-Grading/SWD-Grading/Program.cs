
using Amazon.S3;
using BLL.Interface;
using BLL.Mapper;
using BLL.Service;
using DAL;
using DAL.Interface;
using DAL.Repository;
using Microsoft.EntityFrameworkCore;

namespace SWD_Grading
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
			{
				options.MultipartBodyLengthLimit = 524288000; 
			});

			builder.WebHost.ConfigureKestrel(options =>
			{
				options.Limits.MaxRequestBodySize = 524288000; 
			});

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			// Database Context
			builder.Services.AddDbContext<SWDGradingDbContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
			
			// Unit of Work and Generic Repository
			builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
			builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

		// AWS S3 Configuration
		var awsOptions = builder.Configuration.GetAWSOptions();
		var awsConfig = builder.Configuration.GetSection("AWS");
		
		awsOptions.Credentials = new Amazon.Runtime.BasicAWSCredentials(
			awsConfig["AccessKey"],
			awsConfig["SecretKey"]
		);
		awsOptions.Region = Amazon.RegionEndpoint.GetBySystemName(awsConfig["Region"]);
		
		builder.Services.AddDefaultAWSOptions(awsOptions);
		builder.Services.AddAWSService<IAmazonS3>();

			// Services
			builder.Services.AddScoped<IAuthService, AuthService>();
			builder.Services.AddScoped<IExamService, ExamService>();
			builder.Services.AddScoped<ITesseractOcrService>(sp =>
			{
				var tessdataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
				var uow = sp.GetRequiredService<IUnitOfWork>();

				return new TesseractOcrService(tessdataPath, uow);
			});
			builder.Services.AddScoped<IS3Service, S3Service>();
			builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
			builder.Services.AddScoped<IExamUploadService, ExamUploadService>();
			builder.Services.AddHostedService<BackgroundJobService>();

			// Repositories
			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddScoped<IStudentRepository, StudentRepository>();
			builder.Services.AddScoped<IExamRepository, ExamRepository>();
			builder.Services.AddScoped<IExamZipRepository, ExamZipRepository>();
			builder.Services.AddScoped<IExamStudentRepository, ExamStudentRepository>();
			builder.Services.AddScoped<IDocFileRepository, DocFileRepository>();

			// AutoMapper
			builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);
			builder.Services.AddAutoMapper(typeof(ExamProfile).Assembly);

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
