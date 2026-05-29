using Agility.Zoey.Data.DbContext;
using Agility.Zoey.Data.Repository;
using Agility.Zoey.Data.SeedData;
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

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddSingleton<SeedDataInitializer>();

        return services;
    }
}