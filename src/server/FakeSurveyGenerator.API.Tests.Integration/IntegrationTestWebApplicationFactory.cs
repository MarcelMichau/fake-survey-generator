using System;
using System.Linq;
using System.Threading;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Data;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    public sealed class IntegrationTestWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            SetupEnvironmentVariables();

            builder.ConfigureServices(services =>
            {
                RemoveDefaultDbContextFromServiceCollection(services);
                RemoveDefaultDistributedCacheFromServiceCollection(services);

                services.AddDistributedMemoryCache();

                services.AddDbContext<SurveyContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                ConfigureMockServices(services);

                var rootServiceProvider = services.BuildServiceProvider();

                using var scope = rootServiceProvider.CreateScope();

                var scopedServiceProvider = scope.ServiceProvider;
                var context = scopedServiceProvider.GetRequiredService<SurveyContext>();
                var logger = scopedServiceProvider
                    .GetRequiredService<ILogger<IntegrationTestWebApplicationFactory<TStartup>>>();

                var cache = scopedServiceProvider.GetRequiredService<IDistributedCache>();

                cache.Remove("FakeSurveyGenerator");

                context.Database.EnsureCreated();

                try
                {
                    DatabaseSeed.SeedSampleData(context);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the database with test surveys. Error: {Message}",
                        ex.Message);
                }
            });
        }

        private static void SetupEnvironmentVariables()
        {
            SetEnvironmentVariableIfEmpty("ConnectionStrings__SurveyContext",
                "Server=sqlserver;Database=FakeSurveyGenerator;user id=SA;pwd=<YourStrong!Passw0rd>;ConnectRetryCount=0");
            SetEnvironmentVariableIfEmpty("IDENTITY_PROVIDER_URL", "https://test.com");
        }

        private static void ConfigureMockServices(IServiceCollection services)
        {
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TestUser());
            mockUserService.Setup(service => service.GetUserIdentity())
                .Returns(new TestUser().Id);

            services.AddScoped(sp => mockUserService.Object);
        }

        private static void SetEnvironmentVariableIfEmpty(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
                Environment.SetEnvironmentVariable(key, value);
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