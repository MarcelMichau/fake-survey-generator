using System;
using System.Threading.Tasks;
using AutoMapper;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests
{
    public sealed class QueryTestFixture : IAsyncLifetime, IDisposable
    {
        public SurveyContext Context { get; }
        public IMapper Mapper { get; }

        public QueryTestFixture()
        {
            Context = SurveyContextFactory.Create();

            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            Mapper = configurationProvider.CreateMapper();
        }

        public void Dispose()
        {
            SurveyContextFactory.Destroy(Context);
        }

        public async Task InitializeAsync()
        {
            await SurveyContextFactory.SeedSampleData(Context);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }

    [CollectionDefinition(nameof(QueryTestFixture))]
    public class QueryCollection : ICollectionFixture<QueryTestFixture> { }
}
