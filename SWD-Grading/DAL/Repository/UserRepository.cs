using DAL.Interface;
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
	}
}
