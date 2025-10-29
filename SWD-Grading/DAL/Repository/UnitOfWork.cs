using DAL.Interface;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly SWDGradingDbContext _context;
		private readonly Hashtable _repos = new();

		public UnitOfWork(SWDGradingDbContext context)
		{
			_context = context;
		}
		public IGenericRepository<T, TId> GetRepository<T, TId>()
			where T : class
			where TId : notnull
		{
			var typeName = typeof(T).Name;
			if (_repos.ContainsKey(typeName))
				return (IGenericRepository<T, TId>)_repos[typeName]!;

			var repoInstance = new GenericRepository<T, TId>(_context);
			_repos.Add(typeName, repoInstance);
			return repoInstance;
		}

		public async Task<int> SaveChangesAsync()
			=> await _context.SaveChangesAsync();

		public async Task<IDbContextTransaction> BeginTransactionAsync()
			=> await _context.Database.BeginTransactionAsync();

		private IUserRepository? _userRepository;
		public IUserRepository UserRepository
		{
			get
			{
				if (_userRepository == null)
				{
					_userRepository = new UserRepository(_context);
				}
				return _userRepository;
			}
		}

	}
}
