using DAL.Interface;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
	public class UserRepository : GenericRepository<User, int>, IUserRepository
	{
		private readonly SWDGradingDbContext _context;
		public UserRepository(SWDGradingDbContext context) : base(context)
		{
			_context = context;
		}

        public async Task<User?> GetByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> IsUsernameExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.Username == username);
        }
		public async Task<User?> GetByTeacherCodeAsync(string teacherCode)
		{
			return await _context.Users
				.FirstOrDefaultAsync(u => u.TeacherCode == teacherCode);
		}
	}
}
