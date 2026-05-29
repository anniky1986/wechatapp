using Agility.Zoey.Data.DbContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace Agility.Zoey.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<SqlSugarScope>(sp =>
        {
            return SqlSugarDbContext.CreateClient(sp, configuration);
        });

        return services;
    }
}