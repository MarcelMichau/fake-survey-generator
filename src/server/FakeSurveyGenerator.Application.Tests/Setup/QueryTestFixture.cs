using AutoMapper;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Mappings;

namespace FakeSurveyGenerator.Application.Tests.Setup;

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