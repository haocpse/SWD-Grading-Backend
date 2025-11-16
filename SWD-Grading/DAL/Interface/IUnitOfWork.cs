using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interface
{
	public interface IUnitOfWork
	{
		IGenericRepository<T, TId> GetRepository<T, TId>()
			where T : class
			where TId : notnull;

		IUserRepository UserRepository { get; }
		IExamRepository ExamRepository { get; }
		Task<int> SaveChangesAsync();
		Task<IDbContextTransaction> BeginTransactionAsync();
	}
}
