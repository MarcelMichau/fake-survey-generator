using FakeSurveyGenerator.Api.Tests.Integration.Setup;
using FakeSurveyGenerator.Application.Shared.Caching;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

[Collection(nameof(IntegrationTestFixture))]
public sealed class CacheTests(IntegrationTestFixture testFixture)
{
    private readonly IntegrationTestWebApplicationFactory? _clientFactory = testFixture.Factory;

    [Fact]
    public async Task GivenADistributedCache_WhenGettingAnItemThatIsNotCached_ThenCachedValueShouldBeNull()
    {
        var cache = _clientFactory!.Services.GetRequiredService<ICache<string>>();

        const string cacheKey = "brand-new-key";

        var cachedValue = await cache.GetAsync(cacheKey, CancellationToken.None);

        cachedValue.Should().BeNull();
    }

    [Fact]
    public async Task GivenADistributedCache_WhenGettingAnItemThatIsCached_ThenCachedValueShouldBeReturned()
    {
        var cache = _clientFactory!.Services.GetRequiredService<ICache<string>>();

        const string cacheKey = "test-key";
        const string expectedValue = "test-value";

        await cache.SetAsync(cacheKey, expectedValue, 1, CancellationToken.None);

        var cachedValue = await cache.GetAsync(cacheKey, CancellationToken.None);

        cachedValue.Should().Be(expectedValue);
    }

    [Fact]
    public async Task GivenADistributedCache_WhenRemovingAnItemFromCache_ThenItemShouldNoLongerBeInCache()
    {
        var cache = _clientFactory!.Services.GetRequiredService<ICache<string>>();

        const string cacheKey = "test-key";

        await cache.SetAsync(cacheKey, "test-value", 1, CancellationToken.None);

        await cache.RemoveAsync(cacheKey, CancellationToken.None);

        var cachedValue = await cache.GetAsync(cacheKey, CancellationToken.None);

        cachedValue.Should().BeNull();
    }
}