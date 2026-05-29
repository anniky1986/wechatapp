using Agility.Zoey.Core.Interfaces;
using Agility.Zoey.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace Agility.Zoey.Data.Filters;

public static class DataPermissionFilter
{
    public static void ConfigureDataPermissionFilter(SqlSugarScopeProvider client, IServiceProvider serviceProvider)
    {
        var currentUserAccessor = serviceProvider.GetService<ICurrentUserAccessor>();

        client.QueryFilter.AddTableFilter<ISkipDataPermission>(_ => true, isJoinQuery: true);
        client.QueryFilter.AddTableFilter<ISkipAllDataFilter>(_ => true, isJoinQuery: true);
    }
}