using FakeSurveyGenerator.Application.Infrastructure.Persistence;

namespace FakeSurveyGenerator.Application.Tests.Setup;

public sealed class QueryTestFixture : IAsyncLifetime, IDisposable
{
    public SurveyContext Context { get; } = SurveyContextFactory.Create();

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