using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using TUnit.Core.Interfaces;

namespace FakeSurveyGenerator.Api.Tests.Integration.Setup;

public class IntegrationTestFixture : IAsyncInitializer, IAsyncDisposable
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(300);

    private TestingAspireAppHost? _appHost;
    private IServiceScopeFactory? _serviceScopeFactory;
    public IntegrationTestWebApplicationFactory? Factory;

    public async Task InitializeAsync()
    {
        _appHost = new TestingAspireAppHost();
        await _appHost.StartAsync();

        await _appHost.App!.ResourceNotifications
            .WaitForResourceHealthyAsync("database")
            .WaitAsync(DefaultTimeout);

        await _appHost.App!.ResourceNotifications
            .WaitForResourceHealthyAsync("cache")
            .WaitAsync(DefaultTimeout);

        var sqlConnectionString = await _appHost.GetConnectionString("database");
        var cacheConnectionString = await _appHost.GetConnectionString("cache");

        Factory = new IntegrationTestWebApplicationFactory(
            new AspireTestSettings(sqlConnectionString!, cacheConnectionString!));

        _serviceScopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();

        using var scope = _serviceScopeFactory.CreateScope();

        var scopedServiceProvider = scope.ServiceProvider;

        var context = scopedServiceProvider.GetRequiredService<SurveyContext>();

        await context.Database.MigrateAsync();

        await using var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        var respawner = await Respawner.CreateAsync(connection);

        var cache = scopedServiceProvider.GetRequiredService<IDistributedCache>();
        await cache.RemoveAsync("FakeSurveyGenerator");

        await respawner.ResetAsync(connection);
        await connection.CloseAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_appHost != null) await _appHost.DisposeAsync();

        if (Factory != null) await Factory.DisposeAsync();
    }
}
