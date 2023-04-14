using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Data;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using StackExchange.Redis;

namespace FakeSurveyGenerator.API.Tests.Integration;

public sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<IApiMarker>
{
    private readonly TestContainerSettings _settings;

    public IntegrationTestWebApplicationFactory(TestContainerSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"ASPNETCORE_ENVIRONMENT", "Production"}, // Run integration tests as close as possible to how code will be run in Production
                {"SKIP_DAPR", "true"}, // Do not configure Dapr components for integration tests
                
                // The below settings are the minimum required config to "bootstrap" the host so that services which reference these config values don't throw errors
                // They are dummy values & will get overridden in the call to builder.ConfigureTestServices() below
                {"Cache:RedisPassword", "testing"},
                {"Cache:RedisSsl", "false"},
                {"Cache:RedisUrl", "127.0.0.1"},
                {"Cache:RedisDefaultDatabase", "0"},
                {"IDENTITY_PROVIDER_URL", "https://somenonexistentdomain.com"}
            }!);
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureTestServices(services =>
        {
            RemoveDefaultDbContextFromServiceCollection(services);
            RemoveDefaultDistributedCacheFromServiceCollection(services);

            services.AddDbContext<SurveyContext>(options =>
            {
                options.UseSqlServer(_settings.SqlServerConnectionString);
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = ConfigurationOptions.Parse(_settings.RedisConnectionString);
            });

            ConfigureMockServices(services);
        });
    }

    private static void ConfigureMockServices(IServiceCollection services)
    {
        var mockUserService = Substitute.For<IUserService>();
        mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(new TestUser());
        mockUserService.GetUserIdentity().Returns(new TestUser().Id);

        services.AddScoped(_ => mockUserService);
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