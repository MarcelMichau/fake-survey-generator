using System.Threading.Tasks;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration;

[CollectionDefinition(nameof(IntegrationTestFixture))]
public class IntegrationTestFixtureCollection : ICollectionFixture<IntegrationTestFixture> { }

public class IntegrationTestFixture : IAsyncLifetime
{
    public readonly IntegrationTestWebApplicationFactory Factory;

    private readonly Checkpoint _checkpoint;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public IntegrationTestFixture()
    {
        Factory = new IntegrationTestWebApplicationFactory();
        _checkpoint = new Checkpoint();
        _configuration = Factory.Services.GetRequiredService<IConfiguration>();
        _serviceScopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    public async Task InitializeAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var scopedServiceProvider = scope.ServiceProvider;

        var context = scopedServiceProvider.GetRequiredService<SurveyContext>();
        await context.Database.EnsureCreatedAsync();

        var cache = scopedServiceProvider.GetRequiredService<IDistributedCache>();
        await cache.RemoveAsync("FakeSurveyGenerator");

        if (_configuration.GetValue<bool>("USE_REAL_DEPENDENCIES"))
        {
            var connectionString = _configuration.GetConnectionString(nameof(SurveyContext));
            await _checkpoint.Reset(connectionString);
        }
    }

    public Task DisposeAsync()
    {
        Factory?.Dispose();
        return Task.CompletedTask;
    }
}