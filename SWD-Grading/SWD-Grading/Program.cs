
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

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			builder.Services.AddDbContext<SWDGradingDbContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
			builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
			builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

			//service
			builder.Services.AddScoped<IAuthService, AuthService>();

			//repository
			builder.Services.AddScoped<IUserRepository, UserRepository>();

			//mapper
			builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);

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
