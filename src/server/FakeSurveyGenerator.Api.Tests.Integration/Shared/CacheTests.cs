using FakeSurveyGenerator.Api.Tests.Integration.Setup;
using FakeSurveyGenerator.Application.Shared.Caching;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

public sealed class CacheTests
{
    [ClassDataSource<IntegrationTestFixture>(Shared = SharedType.PerTestSession)]
    public required IntegrationTestFixture TestFixture { get; init; }

    private WebApplicationFactory<Program> ClientFactory =>
        TestFixture.Factory!;

    [Test]
    public async Task GivenADistributedCache_WhenGettingAnItemThatIsNotCached_ThenCachedValueShouldBeNull()
    {
        var cache = ClientFactory.Services.GetRequiredService<ICache<string>>();

        const string cacheKey = "brand-new-key";

        var cachedValue = await cache.GetOrCreateAsync(cacheKey, _ => new ValueTask<string>(), CancellationToken.None);

        await Assert.That(cachedValue).IsNull();
    }

    [Test]
    [Retry(3)] // This test is flaky & fails periodically as a result of a suspected cache-miss when using the hybrid cache
    public async Task GivenADistributedCache_WhenGettingAnItemThatIsCached_ThenCachedValueShouldBeReturned()
    {
        var cache = ClientFactory.Services.GetRequiredService<ICache<string>>();

        const string cacheKey = "test-key";
        const string expectedValue = "test-value";

        await cache.SetAsync(cacheKey, expectedValue, 1, CancellationToken.None);

        var cachedValue = await cache.GetOrCreateAsync(cacheKey, _ => new ValueTask<string>(), CancellationToken.None);

        await Assert.That(cachedValue).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task GivenADistributedCache_WhenRemovingAnItemFromCache_ThenItemShouldNoLongerBeInCache()
    {
        var cache = ClientFactory.Services.GetRequiredService<ICache<string>>();

        const string cacheKey = "test-key";

        await cache.SetAsync(cacheKey, "test-value", 1, CancellationToken.None);

        await cache.RemoveAsync(cacheKey, CancellationToken.None);

        var cachedValue = await cache.GetOrCreateAsync(cacheKey, _ => new ValueTask<string>(), CancellationToken.None);

        await Assert.That(cachedValue).IsNull();
    }
}