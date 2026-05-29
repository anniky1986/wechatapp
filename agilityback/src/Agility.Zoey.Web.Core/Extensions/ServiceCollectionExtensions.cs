using Agility.Zoey.Web.Core.Modules.FileManager.Services;
using Agility.Zoey.Web.Core.Modules.FileManager.Services.StorageProviders;
using Agility.Zoey.Web.Core.Modules.System.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Agility.Zoey.Web.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebCoreServices(this IServiceCollection services)
    {
        services.AddTransient<AuthService>();
        services.AddTransient<UserService>();
        services.AddTransient<RoleService>();
        services.AddTransient<MenuService>();
        services.AddTransient<DeptService>();
        services.AddTransient<TenantService>();
        services.AddTransient<DictService>();
        services.AddTransient<SettingService>();
        services.AddTransient<LogService>();
        services.AddTransient<FileStorageService>();
        services.AddTransient<FileFolderService>();

        services.AddTransient<IFileStorageProvider, LocalFileStorageProvider>();
        services.AddTransient<AliyunOSSProvider>();
        services.AddTransient<TencentCOSProvider>();

        services.AddValidatorsFromAssemblyContaining<Agility.Zoey.Web.Core.Modules.System.Validators.AddUserInputValidator>();

        return services;
    }
}