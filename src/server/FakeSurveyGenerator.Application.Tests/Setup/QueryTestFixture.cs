using FakeSurveyGenerator.Application.Infrastructure.Persistence;

namespace FakeSurveyGenerator.Application.Tests.Setup;

public sealed class QueryTestFixture : IAsyncLifetime, IDisposable
{
    public SurveyContext Context { get; } = SurveyContextFactory.Create();

    public async Task InitializeAsync()
    {
        await SurveyContextFactory.SeedSampleData(Context);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        SurveyContextFactory.Destroy(Context);
    }
}

[CollectionDefinition(nameof(QueryTestFixture))]
public class QueryCollection : ICollectionFixture<QueryTestFixture>
{
}