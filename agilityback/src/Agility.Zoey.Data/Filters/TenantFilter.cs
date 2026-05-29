using Agility.Zoey.Core.Interfaces;
using Agility.Zoey.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace Agility.Zoey.Data.Filters;

public static class TenantFilter
{
    public static void ConfigureTenantFilter(SqlSugarScopeProvider client, IServiceProvider serviceProvider)
    {
        var currentUserAccessor = serviceProvider.GetService<ICurrentUserAccessor>();

        client.QueryFilter.AddTableFilter<ITenant>(it =>
        {
            if (currentUserAccessor == null)
            {
                return true;
            }

            var tenantId = currentUserAccessor.GetCurrentTenantId();
            if (tenantId == null)
            {
                return true;
            }

            return it.TenantId == tenantId;
        }, isJoinQuery: true);

        client.QueryFilter.AddTableFilter<ISkipAllDataFilter>(_ => true, isJoinQuery: true);
    }
}