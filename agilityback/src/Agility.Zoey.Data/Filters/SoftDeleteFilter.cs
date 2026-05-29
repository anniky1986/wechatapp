using Agility.Zoey.Core.Interfaces;
using SqlSugar;

namespace Agility.Zoey.Data.Filters;

public static class SoftDeleteFilter
{
    public static void ConfigureSoftDeleteFilter(SqlSugarScopeProvider client)
    {
        client.QueryFilter.AddTableFilter<ISoftDelete>(entity => !entity.IsDeleted, isJoinQuery: true);
    }
}