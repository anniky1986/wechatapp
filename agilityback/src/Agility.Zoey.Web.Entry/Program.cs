using System.Threading.RateLimiting;
using Agility.Zoey.Data.Extensions;
using Agility.Zoey.Data.SeedData;
using Agility.Zoey.Web.Core.Extensions;
using Agility.Zoey.Web.Entry.EntryExtensions;
using Microsoft.AspNetCore.RateLimiting;

namespace Agility.Zoey.Web.Entry;

public class Program
{
    public static void Main(string[] args)
    {
        Serve.Run(RunOptions.Default.ConfigureServices((context, services) =>
        {
            services.AddHttpContextAccessor();

            services.AddJwt();

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("fixed", config =>
                {
                    config.PermitLimit = 100;
                    config.Window = TimeSpan.FromMinutes(1);
                    config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    config.QueueLimit = 0;
                });

                options.RejectionStatusCode = 429;
            });

            if (context.Configuration.GetValue<bool>("Redis:Enabled"))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = context.Configuration.GetValue<string>("Redis:ConnectionString");
                    options.InstanceName = "AgilityZoey:";
                });
            }

            services.AddDataServices(context.Configuration);
            services.AddWebCoreServices();
            services.AddWebEntryServices();
        }).Configure(app =>
        {
            app.UseResponseCompression();
            app.UseRateLimiter();
            app.UseFurion();

            SeedDataInitializer.InitializeAsync(app.ApplicationServices).GetAwaiter().GetResult();
        }));
    }
}