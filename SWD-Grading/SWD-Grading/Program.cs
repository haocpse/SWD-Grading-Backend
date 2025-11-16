
using Amazon.S3;
using BLL.Interface;
using BLL.Mapper;
using BLL.Service;
using DAL;
using DAL.Interface;
using DAL.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

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

            // CORS Configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });

                // Hoặc nếu bạn muốn cấu hình cụ thể hơn cho môi trường production
                //options.AddPolicy("ProductionPolicy", policy =>
                //{
                //    policy.WithOrigins(
                //            "http://localhost:3000",  // React app
                //            "http://localhost:4200",  // Angular app
                //            "https://yourdomain.com"  // Production domain
                //          )
                //          .AllowAnyMethod()
                //          .AllowAnyHeader()
                //          .AllowCredentials();
                //});
            });

            // JWT Configuration
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            // Database Context
            builder.Services.AddDbContext<SWDGradingDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Authentication Configuration
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

            // Swagger Configuration with JWT
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SWD Grading API",
                    Version = "v1",
                    Description = "API for SWD Grading System"
                });

                // Add JWT Authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

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
				var s3 = sp.GetRequiredService<IS3Service>();

				return new TesseractOcrService(tessdataPath, uow, s3);
			});
			builder.Services.AddScoped<IAuthService, AuthService>();
		builder.Services.AddScoped<IExamService, ExamService>();
		builder.Services.AddScoped<IExamStudentService, ExamStudentService>();
		builder.Services.AddScoped<IS3Service, S3Service>();
		builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
		builder.Services.AddScoped<IExamUploadService, ExamUploadService>();
		builder.Services.AddScoped<IVectorService, VectorService>();
		builder.Services.AddScoped<IAIVerificationService, AIVerificationService>();
		builder.Services.AddScoped<IPlagiarismService, PlagiarismService>();
		
		// Register BackgroundJobService to automatically process uploaded ZIP files
		builder.Services.AddHostedService<BackgroundJobService>();

			// Repositories
			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddScoped<IStudentRepository, StudentRepository>();
			builder.Services.AddScoped<IExamRepository, ExamRepository>();
			builder.Services.AddScoped<IExamZipRepository, ExamZipRepository>();
			builder.Services.AddScoped<IExamStudentRepository, ExamStudentRepository>();
			builder.Services.AddScoped<IDocFileRepository, DocFileRepository>();
			builder.Services.AddScoped<ISimilarityCheckRepository, SimilarityCheckRepository>();
            builder.Services.AddScoped<IRubricRepository, RubricRepository>();
            builder.Services.AddScoped<IExamQuestionRepository, ExamQuestionRepository>();

			// AutoMapper
			builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);
			builder.Services.AddAutoMapper(typeof(ExamProfile).Assembly);
			builder.Services.AddAutoMapper(typeof(ExamQuestionProfile).Assembly);
			builder.Services.AddAutoMapper(typeof(RubricProfile).Assembly);
			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

            app.UseCors("AllowAll");

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
