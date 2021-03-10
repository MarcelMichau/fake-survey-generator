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
            // When running the integration tests with Docker Compose, the USE_ENVIRONMENT_VARIABLES_ONLY is set to true & the environment variables used
            // in the docker-compose-tests.override.yml file are used.
            // Integration tests can be run with Docker Compose by running the following in a terminal:
            // docker-compose -f docker-compose-tests.yml -f docker-compose-tests.override.yml up --build fake-survey-generator-api-integration-test

            // When running the integration tests with `dotnet test` or through Visual Studio, the USE_ENVIRONMENT_VARIABLES_ONLY is not set, therefore
            // any necessary config is set by using builder.ConfigureAppConfiguration().

            if (!Convert.ToBoolean(Environment.GetEnvironmentVariable("USE_ENVIRONMENT_VARIABLES_ONLY")))
            {
                builder.ConfigureAppConfiguration(config =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        // To test with real dependencies for SQL Server & Redis outside of Docker Compose, the USE_REAL_DEPENDENCIES setting can be set to true here.
                        // Ensure that the SQL Server & Redis docker containers are running before starting the test run by running the following in a terminal:
                        // docker-compose -f docker-compose.yml -f docker-compose.override.yml up --build sql-server redis

                        // When running with USE_REAL_DEPENDENCIES set to false, an in-memory database & an in-memory distributed cache will be used instead.

                        {"ASPNETCORE_ENVIRONMENT", "Production" }, // Run integration tests as close as possible to how code will be run in Production
                        {"USE_REAL_DEPENDENCIES", "false" },
                        {
                            "ConnectionStrings:SurveyContext",
                            "Server=127.0.0.1;Database=FakeSurveyGenerator;user id=SA;pwd=<YourStrong!Passw0rd>;ConnectRetryCount=0"
                        },
                        {"REDIS_PASSWORD", "testing"},
                        {"REDIS_SSL", "false"},
                        {"REDIS_URL", "127.0.0.1"},
                        {"REDIS_DEFAULT_DATABASE", "0"},
                        {"IDENTITY_PROVIDER_URL", "https://somenonexistentdomain.com"}
                    });
                });
            }

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