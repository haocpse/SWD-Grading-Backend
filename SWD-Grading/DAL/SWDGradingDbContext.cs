using Microsoft.EntityFrameworkCore;
using Model.Entity;

namespace DAL
{
	public class SWDGradingDbContext : DbContext
	{
		public SWDGradingDbContext(DbContextOptions<SWDGradingDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
		}
	}
}
