using System;
using System.Threading.Tasks;
using FakeSurveyGenerator.Data;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Respawn;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    [CollectionDefinition(nameof(IntegrationTestFixture))]
    public class IntegrationTestFixtureCollection : ICollectionFixture<IntegrationTestFixture> { }

    public class IntegrationTestFixture : IAsyncLifetime
    {
        public readonly IntegrationTestWebApplicationFactory<Startup> Factory;

        private Checkpoint _checkpoint;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public IntegrationTestFixture()
        {
            Factory = new IntegrationTestWebApplicationFactory<Startup>();
            _configuration = Factory.Services.GetRequiredService<IConfiguration>();
            _serviceScopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();
        }

        public async Task InitializeAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var scopedServiceProvider = scope.ServiceProvider;
            var context = scopedServiceProvider.GetRequiredService<SurveyContext>();

            var logger = scopedServiceProvider
                .GetRequiredService<ILogger<IntegrationTestWebApplicationFactory<Startup>>>();

            await context.Database.EnsureCreatedAsync();

            var cache = scopedServiceProvider.GetRequiredService<IDistributedCache>();

            await cache.RemoveAsync("FakeSurveyGenerator");

            try
            {
                _checkpoint = new Checkpoint();
                await DatabaseSeed.SeedSampleData(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the database with test surveys. Error: {Message}",
                    ex.Message);
            }

            var connectionString = _configuration.GetConnectionString(nameof(SurveyContext));
            await _checkpoint.Reset(connectionString);
        }

        public Task DisposeAsync()
        {
            Factory?.Dispose();
            return Task.CompletedTask;
        }
    }
}
