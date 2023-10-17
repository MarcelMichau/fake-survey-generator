﻿using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Respawn;
using Testcontainers.MsSql;
using Testcontainers.Redis;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration.Setup;

[CollectionDefinition(nameof(IntegrationTestFixture))]
public class IntegrationTestFixtureCollection : ICollectionFixture<IntegrationTestFixture> { }

public class IntegrationTestFixture : IAsyncLifetime
{
    public IntegrationTestWebApplicationFactory? Factory;

    private IServiceScopeFactory? _serviceScopeFactory;

    private readonly MsSqlContainer _dbContainer =
        new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:latest")
            .Build();

    private readonly RedisContainer _cacheContainer =
        new RedisBuilder()
            .WithImage("redis:latest")
            .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _cacheContainer.StartAsync();

        var connectionString = _dbContainer.GetConnectionString();

        Factory = new IntegrationTestWebApplicationFactory(new TestContainerSettings(connectionString, _cacheContainer.GetConnectionString()));

        _serviceScopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();

        using var scope = _serviceScopeFactory.CreateScope();

        var scopedServiceProvider = scope.ServiceProvider;

        var context = scopedServiceProvider.GetRequiredService<SurveyContext>();

        await context.Database.MigrateAsync();

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