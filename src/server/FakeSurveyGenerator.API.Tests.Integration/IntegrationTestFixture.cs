using DotNet.Testcontainers.Builders;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Respawn;
using Testcontainers.MsSql;
using Testcontainers.Redis;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration;

[CollectionDefinition(nameof(IntegrationTestFixture))]
public class IntegrationTestFixtureCollection : ICollectionFixture<IntegrationTestFixture> { }

public class IntegrationTestFixture : IAsyncLifetime
{
    public IntegrationTestWebApplicationFactory? Factory;

    private IServiceScopeFactory? _serviceScopeFactory;

    private readonly MsSqlContainer _dbContainer =
        new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:latest")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("SA_PASSWORD", "<YourStrong!Passw0rd>")
            .WithPortBinding(1433, 1433)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .Build();

    private readonly RedisContainer _cacheContainer =
        new RedisBuilder()
            .WithImage("redis:latest")
            .WithPortBinding(6379, 6379)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
            .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _cacheContainer.StartAsync();

        var connectionString =
            $"Server={_dbContainer.Hostname};Database=FakeSurveyGenerator;user id=SA;pwd=<YourStrong!Passw0rd>;ConnectRetryCount=0;Encrypt=false";

        Factory = new IntegrationTestWebApplicationFactory(new TestContainerSettings(connectionString, _cacheContainer.GetConnectionString()));
        
        _serviceScopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();

        using var scope = _serviceScopeFactory.CreateScope();

        var scopedServiceProvider = scope.ServiceProvider;

        var context = scopedServiceProvider.GetRequiredService<SurveyContext>();
        await context.Database.EnsureCreatedAsync();

        var respawner = await Respawner.CreateAsync(connectionString);

        var cache = scopedServiceProvider.GetRequiredService<IDistributedCache>();
        await cache.RemoveAsync("FakeSurveyGenerator");

        await respawner.ResetAsync(connectionString);
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _cacheContainer.DisposeAsync();

        if (Factory != null) await Factory.DisposeAsync();
    }
}