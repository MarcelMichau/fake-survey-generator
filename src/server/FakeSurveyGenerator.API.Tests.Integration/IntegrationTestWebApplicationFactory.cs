using System;
using System.Linq;
using FakeSurveyGenerator.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    public class IntegrationTestWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__SurveyContext", "Server=sqlserver;Database=FakeSurveyGenerator;user id=SA;pwd=<YourStrong!Passw0rd>;ConnectRetryCount=0");
            Environment.SetEnvironmentVariable("IDENTITY_PROVIDER_BACKCHANNEL_URL", "http://test.com");
            Environment.SetEnvironmentVariable("IDENTITY_PROVIDER_FRONTCHANNEL_URL", "http://localhost");

            builder.ConfigureServices(services =>
            {
                RemoveDefaultDbContextFromServiceCollection(services);
                RemoveDefaultDistributedCacheFromServiceCollection(services);

                services.AddDistributedMemoryCache();

                services.AddDbContext<SurveyContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                var serviceProvider = services.BuildServiceProvider();

                using var scope = serviceProvider.CreateScope();

                var scopedServices = scope.ServiceProvider;
                var context = scopedServices.GetRequiredService<SurveyContext>();
                var logger = scopedServices
                    .GetRequiredService<ILogger<IntegrationTestWebApplicationFactory<TStartup>>>();

                var cache = scopedServices.GetService<IDistributedCache>();

                cache.Remove("FakeSurveyGenerator");

                context.Database.EnsureCreated();

                try
                {
                    DatabaseSeed.SeedSampleData(context);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the database with test surveys. Error: {Message}", ex.Message);
                }
            });
        }

        private static void RemoveDefaultDbContextFromServiceCollection(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SurveyContext>));
            if (descriptor == null) return;
            services.Remove(descriptor);
        }

        private static void RemoveDefaultDistributedCacheFromServiceCollection(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDistributedCache));
            if (descriptor == null) return;
            services.Remove(descriptor);
        }
    }
}
