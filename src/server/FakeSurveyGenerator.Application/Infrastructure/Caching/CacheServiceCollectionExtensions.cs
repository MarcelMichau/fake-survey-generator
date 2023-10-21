using FakeSurveyGenerator.Application.Shared.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace FakeSurveyGenerator.Application.Infrastructure.Caching;

internal static class CacheServiceCollectionExtensions
{
    public static IServiceCollection AddCacheConfiguration(this IServiceCollection services,
        CacheOptions cacheOptions)
    {
        services.TryAddSingleton(typeof(ICache<>), typeof(Cache<>));
        services.TryAddSingleton<ICacheFactory, CacheFactory>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = new ConfigurationOptions
            {
                EndPoints = {cacheOptions.RedisUrl ?? throw new InvalidOperationException("RedisUrl was not specified in config")},
                Password = cacheOptions.RedisPassword,
                Ssl = cacheOptions.RedisSsl,
                DefaultDatabase = cacheOptions.RedisDefaultDatabase
            };
        });

        return services;
    }
}