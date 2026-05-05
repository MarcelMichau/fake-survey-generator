using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using TUnit.Core.Interfaces;

namespace FakeSurveyGenerator.Application.Tests.Setup;

public sealed class TestFixture : IAsyncInitializer, IAsyncDisposable
{
    public SurveyContext Context { get; } = SurveyContextFactory.Create();

    public static TestHybridCache GetHybridCache() => new();

    public async Task InitializeAsync()
    {
        await SurveyContextFactory.SeedSampleData(Context);
    }

    public ValueTask DisposeAsync()
    {
        SurveyContextFactory.Destroy(Context);
        return ValueTask.CompletedTask;
    }
}