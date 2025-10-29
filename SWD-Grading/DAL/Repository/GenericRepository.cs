using DAL.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
	public class GenericRepository<T, TId> : IGenericRepository<T, TId>
		where T : class
		where TId : notnull
	{
		private readonly SWDGradingDbContext _dbContext;
		private readonly DbSet<T> _dbSet;

		public GenericRepository(SWDGradingDbContext dbContext)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_dbSet = _dbContext.Set<T>();
		}

		public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(entity);
			await _dbSet.AddAsync(entity, cancellationToken);
		}

		public async Task<int> CountAsync(IEnumerable<Expression<Func<T, bool>>>? filters = null, CancellationToken cancellationToken = default)
		{
			IQueryable<T> query = _dbSet;
			if (filters is not null)
			{
				foreach (var predicate in filters)
				{
					if (predicate is null)
						throw new ArgumentNullException(nameof(filters), "One of the filter expressions is null.");
					query = query.Where(predicate);
				}
			}
			return await query.CountAsync(cancellationToken);
		}


		public async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
		{
			return await _dbSet.FindAsync([id], cancellationToken);
		}

		public async Task<IEnumerable<TResult>> GetPagedAsync<TResult>(int skip, int take, IEnumerable<Expression<Func<T, bool>>>? filters = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Expression<Func<T, TResult>>? selector = null, bool asNoTracking = true, CancellationToken cancellationToken = default)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(skip);
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);

			IQueryable<T> query = _dbSet;

			if (filters is not null)
			{
				foreach (var predicate in filters)
				{
					if (predicate is null)
						throw new ArgumentNullException(nameof(filters), "One of the filter expressions is null.");
					query = query.Where(predicate);
				}
			}

			if (include is not null)
			{
				query = include(query);
			}

			query = orderBy != null ? orderBy(query) : query.OrderBy(_ => true);
			query = query.Skip(skip).Take(take);

			var finalQuery = asNoTracking ? query.AsNoTracking() : query;
			return selector != null
				? await finalQuery.Select(selector).ToListAsync(cancellationToken)
				: await finalQuery.Cast<TResult>().ToListAsync(cancellationToken);
		}

		public async Task<int> GetTotalPagesAsync(int pageSize, IEnumerable<Expression<Func<T, bool>>>? filters = null, CancellationToken cancellationToken = default)
		{
			if (pageSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(pageSize), "pageSize must be greater than zero.");

			var totalCount = await CountAsync(filters, cancellationToken);
			return (int)Math.Ceiling(totalCount / (double)pageSize);
		}

		public virtual IQueryable<T> Query(bool asNoTracking = true)
			 => asNoTracking ? _dbSet.AsNoTracking() : _dbSet;

		public virtual Task RemoveAsync(T entity, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(entity);
			_dbSet.Remove(entity);
			return Task.CompletedTask;
		}

		public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(entity);
			_dbSet.Update(entity);
			return Task.CompletedTask;
		}

	}
}
