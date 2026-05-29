using Agility.Zoey.Data.Extensions;
using Agility.Zoey.Data.SeedData;
using Agility.Zoey.Web.Core.Extensions;
using Agility.Zoey.Web.Entry.EntryExtensions;

namespace Agility.Zoey.Web.Entry;

public class Program
{
    public static void Main(string[] args)
    {
        Serve.Run(RunOptions.Default.ConfigureServices((context, services) =>
        {
            services.AddHttpContextAccessor();

            services.AddJwt();

            services.AddDataServices(context.Configuration);
            services.AddWebCoreServices();
            services.AddWebEntryServices();
        }).Configure(app =>
        {
            app.UseFurion();

            SeedDataInitializer.InitializeAsync(app.ApplicationServices).GetAwaiter().GetResult();
        }));
    }
}