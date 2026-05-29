using Agility.Zoey.Web.Core.Filters;
using Agility.Zoey.Web.Core.Modules.FileManager.Services.StorageProviders;
using Agility.Zoey.Web.Core.Shared.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Agility.Zoey.Web.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebCoreServices(this IServiceCollection services)
    {
        services.AddTransient<IFileStorageProvider, LocalFileStorageProvider>();
        services.AddTransient<AliyunOSSProvider>();
        services.AddTransient<TencentCOSProvider>();

        services.AddValidatorsFromAssemblyContaining<Agility.Zoey.Web.Core.Modules.System.Validators.AddUserInputValidator>();

        services.AddScoped<CacheService>();
        services.AddScoped<PermissionFilter>();

        return services;
    }
}