using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Data;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    public sealed class IntegrationTestWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Environment.SetEnvironmentVariable("USE_REAL_DEPENDENCIES", "true");

            //builder.ConfigureAppConfiguration(config =>
            //{
            //    config.AddInMemoryCollection(new Dictionary<string, string>
            //    {
            //        {
            //            "ConnectionStrings:SurveyContext",
            //            "Server=127.0.0.1;Database=FakeSurveyGenerator;user id=SA;pwd=<YourStrong!Passw0rd>;ConnectRetryCount=0"
            //        },
            //        {"REDIS_PASSWORD", "testing"},
            //        {"REDIS_SSL", "false"},
            //        {"REDIS_URL", "127.0.0.1"},
            //        {"REDIS_DEFAULT_DATABASE", "0"},
            //        {"IDENTITY_PROVIDER_URL", "https://test.com"}
            //    });
            //});

            builder.ConfigureServices((hostBuilderContext, services) =>
            {
                if (!hostBuilderContext.Configuration.GetValue<bool>("USE_REAL_DEPENDENCIES"))
                {
                    RemoveDefaultDbContextFromServiceCollection(services);
                    RemoveDefaultDistributedCacheFromServiceCollection(services);

                    services.AddDistributedMemoryCache();

                    services.AddDbContext<SurveyContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });
                }

                ConfigureMockServices(services);
            });
        }

        private static void ConfigureMockServices(IServiceCollection services)
        {
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TestUser());
            mockUserService.Setup(service => service.GetUserIdentity())
                .Returns(new TestUser().Id);

            services.AddScoped(_ => mockUserService.Object);
        }

        private static void RemoveDefaultDbContextFromServiceCollection(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SurveyContext>));
            if (descriptor is null) return;
            services.Remove(descriptor);
        }

        private static void RemoveDefaultDistributedCacheFromServiceCollection(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDistributedCache));
            if (descriptor is null) return;
            services.Remove(descriptor);
        }
    }
}