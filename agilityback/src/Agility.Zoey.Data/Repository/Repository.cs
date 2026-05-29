using System.Linq.Expressions;
using Agility.Zoey.Core.Interfaces;
using Agility.Zoey.Data.Interfaces;
using SqlSugar;

namespace Agility.Zoey.Data.Repository;

public class Repository<T> : IRepository<T>, IScoped where T : class, new()
{
    private readonly SqlSugarScope _client;
    private readonly ICurrentUserAccessor? _currentUserAccessor;

    public Action<ISqlSugarClient>? ApplyCopyTable { get; set; }

    public Repository(SqlSugarScope client, ICurrentUserAccessor? currentUserAccessor = null)
    {
        _client = client;
        _currentUserAccessor = currentUserAccessor;
    }

    public SqlSugarClient Context => _client;

    public async Task<T?> GetByIdAsync(long id)
    {
        return await _client.Queryable<T>().InSingleAsync(id);
    }

    public async Task<List<T>> GetListAsync()
    {
        return await AsQueryable().ToListAsync();
    }

    public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate)
    {
        return await AsQueryable().Where(predicate).ToListAsync();
    }

    public async Task<(List<T> Items, int TotalCount)> GetPageAsync(int pageIndex, int pageSize)
    {
        var query = AsQueryable();
        var totalCount = 0;
        var items = await query.ToPageListAsync(pageIndex, pageSize, ref totalCount);
        return (items, totalCount);
    }

    public async Task<(List<T> Items, int TotalCount)> GetPageAsync(Expression<Func<T, bool>> predicate, int pageIndex, int pageSize)
    {
        var query = AsQueryable().Where(predicate);
        var totalCount = 0;
        var items = await query.ToPageListAsync(pageIndex, pageSize, ref totalCount);
        return (items, totalCount);
    }

    public async Task<T> InsertAsync(T entity)
    {
        return await _client.Insertable(entity).ExecuteReturnEntityAsync();
    }

    public async Task<int> InsertRangeAsync(List<T> entities)
    {
        return await _client.Insertable(entities).ExecuteCommandAsync();
    }

    public async Task<int> UpdateAsync(T entity)
    {
        return await _client.Updateable(entity).ExecuteCommandAsync();
    }

    public async Task<int> DeleteAsync(long id)
    {
        return await _client.Deleteable<T>().In(id).ExecuteCommandAsync();
    }

    public async Task<int> DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        return await _client.Deleteable<T>().Where(predicate).ExecuteCommandAsync();
    }

    public async Task<int> SoftDeleteAsync(long id)
    {
        return await _client.Updateable<T>()
            .SetColumns("IsDeleted", true)
            .Where("Id", "=", id)
            .ExecuteCommandAsync();
    }

    public async Task<int> SoftDeleteAsync(Expression<Func<T, bool>> predicate)
    {
        return await _client.Updateable<T>()
            .SetColumns("IsDeleted", true)
            .Where(predicate)
            .ExecuteCommandAsync();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await AsQueryable().CountAsync(predicate);
    }

    public ISugarQueryable<T> AsQueryable()
    {
        var query = _client.Queryable<T>();
        ApplyCopyTable?.Invoke(_client);
        return query;
    }

    public ISugarQueryable<T> AsSugarQueryable()
    {
        return AsQueryable();
    }

    public Task BeginTranAsync()
    {
        _client.Ado.BeginTran();
        return Task.CompletedTask;
    }

    public Task CommitTranAsync()
    {
        _client.Ado.CommitTran();
        return Task.CompletedTask;
    }

    public Task RollbackTranAsync()
    {
        _client.Ado.RollbackTran();
        return Task.CompletedTask;
    }
}