using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Common.Caching
{
    public sealed class DistributedCache<T> : IDistributedCache<T>
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<DistributedCache<T>> _logger;

        private static string ApplicationName => "FakeSurveyGenerator";
        private readonly string _cacheKeyPrefix;

        public DistributedCache(IDistributedCache distributedCache, ILogger<DistributedCache<T>> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;

            _cacheKeyPrefix = $"{ApplicationName}:{typeof(T).Namespace}:{typeof(T).Name}:";
        }

        public async Task<(bool Found, T Value)> TryGetValueAsync(string key, CancellationToken cancellationToken)
        {
            var value = await GetAsync(key, cancellationToken);

            return (value != null, value);
        }

        public async Task<T> GetAsync(string key, CancellationToken cancellationToken)
        {
            try
            {
                var cachedResult = await _distributedCache.GetAsync(CacheKey(key), cancellationToken);

                return cachedResult == null ? default : await DeserialiseCacheResult(cachedResult, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while trying to Get a value from the cache");
                return default;
            }
        }

        public async Task SetAsync(string key, T item, int minutesToCache, CancellationToken cancellationToken)
        {
            try
            {
                var cacheEntryOptions = new DistributedCacheEntryOptions
                { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(minutesToCache) };

                var serialisedItemToCache = SerialiseForCaching(item);

                await _distributedCache.SetStringAsync(CacheKey(key), serialisedItemToCache, cacheEntryOptions,
                    cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while trying to Set a value in the cache");
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            try
            {
                await _distributedCache.RemoveAsync(CacheKey(key), cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while trying to Remove a value from the cache");
            }
        }

        private string CacheKey(string key) => $"{_cacheKeyPrefix}{key}";

        private async Task<T> DeserialiseCacheResult(byte[] cachedResult, CancellationToken cancellationToken)
        {
            try
            {
                await using var stream = new MemoryStream(cachedResult);

                return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to deserialise from cached value");
                return default;
            }
        }

        private string SerialiseForCaching(T item)
        {
            if (item == null) return null;

            try
            {
                return JsonSerializer.Serialize(item);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to serialise type '{Type}' for caching", typeof(T).FullName);
                throw;
            }
        }
    }
}