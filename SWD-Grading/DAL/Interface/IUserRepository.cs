using Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interface
{
	public interface IUserRepository : IGenericRepository<User, int>
	{
        Task<User?> GetByUsername(string username);
        Task<bool> IsUsernameExists(string username);
		Task<User?> GetByTeacherCodeAsync(string teacherCode);
	}
}
