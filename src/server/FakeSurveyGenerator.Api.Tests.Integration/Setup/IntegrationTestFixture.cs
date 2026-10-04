using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core.Interfaces;

namespace FakeSurveyGenerator.Api.Tests.Integration.Setup;

public class IntegrationTestFixture : IAsyncInitializer, IAsyncDisposable
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(300);

    private TestingAspireAppHost? _appHost;
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

        var serviceScopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();

        using var scope = serviceScopeFactory.CreateScope();

        var scopedServiceProvider = scope.ServiceProvider;

        var context = scopedServiceProvider.GetRequiredService<SurveyContext>();

        // SQL Server and Redis start fresh for the session; tests create their own data.
        await context.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_appHost != null) await _appHost.DisposeAsync();

        if (Factory != null) await Factory.DisposeAsync();
    }
}
