using System.Linq.Expressions;
using SqlSugar;

namespace Agility.Zoey.Data.Repository;

public interface IRepository<T> where T : class, new()
{
    Task<T?> GetByIdAsync(long id);

    Task<List<T>> GetListAsync();

    Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate);

    Task<(List<T> Items, int TotalCount)> GetPageAsync(int pageIndex, int pageSize);

    Task<(List<T> Items, int TotalCount)> GetPageAsync(Expression<Func<T, bool>> predicate, int pageIndex, int pageSize);

    Task<T> InsertAsync(T entity);

    Task<int> InsertRangeAsync(List<T> entities);

    Task<int> UpdateAsync(T entity);

    Task<int> DeleteAsync(long id);

    Task<int> DeleteAsync(Expression<Func<T, bool>> predicate);

    Task<int> SoftDeleteAsync(long id);

    Task<int> SoftDeleteAsync(Expression<Func<T, bool>> predicate);

    Task<int> CountAsync(Expression<Func<T, bool>> predicate);

    ISugarQueryable<T> AsQueryable();

    ISugarQueryable<T> AsSugarQueryable();

    SqlSugarClient Context { get; }

    Task BeginTranAsync();

    Task CommitTranAsync();

    Task RollbackTranAsync();
}