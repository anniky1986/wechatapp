using Agility.Zoey.Core.Entities;
using Agility.Zoey.Core.Enums;
using Agility.Zoey.Core.Interfaces;
using Agility.Zoey.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace Agility.Zoey.Data.Filters;

public static class DataPermissionFilter
{
    private static IServiceProvider _serviceProvider;

    public static void ConfigureDataPermissionFilter(SqlSugarScopeProvider client, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        client.QueryFilter.AddTableFilter<ISkipDataPermission>(_ => true, isJoinQuery: true);
        client.QueryFilter.AddTableFilter<ISkipAllDataFilter>(_ => true, isJoinQuery: true);
    }

    public static ISugarQueryable<T> ApplyDataPermission<T>(this ISugarQueryable<T> queryable) where T : class, new()
    {
        if (typeof(ISkipDataPermission).IsAssignableFrom(typeof(T)) ||
            typeof(ISkipAllDataFilter).IsAssignableFrom(typeof(T)))
        {
            return queryable;
        }

        var currentUserAccessor = _serviceProvider?.GetService<ICurrentUserAccessor>();
        if (currentUserAccessor == null)
        {
            return queryable;
        }

        var userId = currentUserAccessor.GetCurrentUserId();
        var tenantId = currentUserAccessor.GetCurrentTenantId();
        if (userId == null)
        {
            return queryable;
        }

        var dataScope = ResolveDataScope(currentUserAccessor).GetAwaiter().GetResult();
        if (dataScope == null || dataScope == (int)DataScope.All)
        {
            return queryable;
        }

        var entityInfo = queryable.Context.EntityMaintenance.GetEntityInfo<T>();
        var columnNames = entityInfo.Columns.Select(c => c.PropertyName).ToHashSet();

        switch ((DataScope)dataScope.Value)
        {
            case DataScope.Dept:
            case DataScope.DeptAndChild:
            case DataScope.Custom:
                if (columnNames.Contains("DeptId"))
                {
                    var deptIds = GetDeptIdsForScope(dataScope.Value, currentUserAccessor).GetAwaiter().GetResult();
                    if (deptIds.Count > 0)
                    {
                        queryable = queryable.Where($"DeptId IN ({string.Join(",", deptIds)})");
                    }
                    else
                    {
                        queryable = queryable.Where(_ => false);
                    }
                }
                break;

            case DataScope.Self:
                if (columnNames.Contains("CreateUserId"))
                {
                    queryable = queryable.Where($"CreateUserId = {userId}");
                }
                break;
        }

        return queryable;
    }

    private static async Task<int?> ResolveDataScope(ICurrentUserAccessor currentUserAccessor)
    {
        var userId = currentUserAccessor.GetCurrentUserId();
        if (userId == null) return null;

        using var scope = _serviceProvider?.CreateScope();
        if (scope == null) return null;

        var db = scope.ServiceProvider.GetRequiredService<SqlSugarScope>();

        var user = await db.Queryable<User>()
            .Where(u => u.Id == userId.Value)
            .Select(u => new { u.DeptId, u.TenantId })
            .FirstAsync();

        if (user == null) return null;

        var roleIds = await db.Queryable<UserRole>()
            .Where(ur => ur.UserId == userId.Value)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (!roleIds.Any()) return null;

        var scopeValues = await db.Queryable<Role>()
            .Where(r => roleIds.Contains(r.Id) && r.Status == 1)
            .Select(r => r.DataScope)
            .ToListAsync();

        if (!scopeValues.Any()) return null;

        return scopeValues.Max();
    }

    private static async Task<List<long>> GetDeptIdsForScope(int dataScope, ICurrentUserAccessor currentUserAccessor)
    {
        var userId = currentUserAccessor.GetCurrentUserId();
        if (userId == null) return new List<long>();

        using var scope = _serviceProvider?.CreateScope();
        if (scope == null) return new List<long>();

        var db = scope.ServiceProvider.GetRequiredService<SqlSugarScope>();

        var user = await db.Queryable<User>()
            .Where(u => u.Id == userId.Value)
            .Select(u => new { u.DeptId, u.TenantId })
            .FirstAsync();

        if (user == null) return new List<long>();

        var userDeptId = user.DeptId;
        var tenantId = user.TenantId;

        switch ((DataScope)dataScope)
        {
            case DataScope.Dept:
                return new List<long> { userDeptId };

            case DataScope.DeptAndChild:
                var allDepts = await db.Queryable<Dept>()
                    .Where(d => d.TenantId == tenantId)
                    .ToListAsync();
                var childDeptIds = GetAllChildDeptIds(allDepts, userDeptId);
                childDeptIds.Add(userDeptId);
                return childDeptIds;

            case DataScope.Custom:
                var roleIds = await db.Queryable<UserRole>()
                    .Where(ur => ur.UserId == userId.Value)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                if (!roleIds.Any()) return new List<long>();

                return await db.Queryable<DataPermission>()
                    .Where(dp => roleIds.Contains(dp.RoleId))
                    .Select(dp => dp.DeptId)
                    .Distinct()
                    .ToListAsync();

            default:
                return new List<long>();
        }
    }

    private static List<long> GetAllChildDeptIds(List<Dept> allDepts, long parentId)
    {
        var childIds = new List<long>();
        var directChildren = allDepts.Where(d => d.ParentId == parentId).ToList();
        foreach (var child in directChildren)
        {
            childIds.Add(child.Id);
            childIds.AddRange(GetAllChildDeptIds(allDepts, child.Id));
        }
        return childIds;
    }
}