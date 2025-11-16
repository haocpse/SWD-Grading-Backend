using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interface
{
	public interface IGenericRepository<T, in TId>
	{
		Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

		Task<IEnumerable<TResult>> GetPagedAsync<TResult>(int skip, int take, IEnumerable<Expression<Func<T, bool>>>? filters = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Expression<Func<T, TResult>>? selector = null, bool asNoTracking = true, CancellationToken cancellationToken = default);

		Task<int> CountAsync(IEnumerable<Expression<Func<T, bool>>>? filters = null, CancellationToken cancellationToken = default);

		Task<int> GetTotalPagesAsync(int pageSize, IEnumerable<Expression<Func<T, bool>>>? filters = null, CancellationToken cancellationToken = default);

		IQueryable<T> Query(bool asNoTracking = true);

		Task AddAsync(T entity, CancellationToken cancellationToken = default);

		Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

		Task RemoveAsync(T entity, CancellationToken cancellationToken = default);

		Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
	}
}
