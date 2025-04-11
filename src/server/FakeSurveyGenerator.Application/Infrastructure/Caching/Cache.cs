using FakeSurveyGenerator.Application.Shared.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Infrastructure.Caching;

internal sealed class Cache<T>(HybridCache cache, ILogger<Cache<T>> logger)
    : ICache<T>
{
    private readonly HybridCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly string _cacheKeyPrefix = $"{typeof(T).Namespace}:{typeof(T).Name}:";

    public async ValueTask<T> GetOrCreateAsync(string key, Func<CancellationToken, ValueTask<T>> factory, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _cache.GetOrCreateAsync(CacheKey(key), factory, cancellationToken: cancellationToken);

            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while either trying to get a value from the cache or calling the factory function");
            throw;
        }
    }

    public async ValueTask SetAsync(string key, T item, int minutesToCache, CancellationToken cancellationToken)
    {
        try
        {
            var cacheEntryOptions = new HybridCacheEntryOptions
                { Expiration = TimeSpan.FromMinutes(minutesToCache) };

            await _cache.SetAsync(CacheKey(key), item, cacheEntryOptions, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while trying to Set a value in the cache");
        }
    }

    public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            await _cache.RemoveAsync(CacheKey(key), cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while trying to Remove a value from the cache");
        }
    }

    private string CacheKey(string key)
    {
        return $"{_cacheKeyPrefix}{key}";
    }
}