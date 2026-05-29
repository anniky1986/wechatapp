using Agility.Zoey.Data.Interfaces;
using Agility.Zoey.Web.Core.Shared.Interfaces;
using Agility.Zoey.Web.Entry.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Agility.Zoey.Web.Entry.EntryExtensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebEntryServices(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}