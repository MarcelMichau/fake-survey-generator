using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Caching;
using NSubstitute;
using TUnit.Core.Interfaces;

namespace FakeSurveyGenerator.Application.Tests.Setup;

public sealed class TestFixture : IAsyncInitializer, IAsyncDisposable
{
    public SurveyContext Context { get; } = SurveyContextFactory.Create();

    public static ICache<T> GetCache<T>()
    {
        var cache = Substitute.For<ICache<T>>();
        cache.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<CancellationToken, ValueTask<T>>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var factory = callInfo.Arg<Func<CancellationToken, ValueTask<T>>>();
                var cancellationToken = callInfo.Arg<CancellationToken>();
                return factory(cancellationToken);
            });
        return cache;
    }

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