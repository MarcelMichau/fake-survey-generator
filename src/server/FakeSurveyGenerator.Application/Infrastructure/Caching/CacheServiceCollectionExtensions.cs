using FakeSurveyGenerator.Application.Shared.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace FakeSurveyGenerator.Application.Infrastructure.Caching;

internal static class CacheServiceCollectionExtensions
{
    internal static readonly string[] RedisTags = {"redis-cache", "ready"};

    public static IServiceCollection AddCacheConfiguration(this IServiceCollection services,
        CacheOptions cacheOptions)
    {
        services.TryAddSingleton(typeof(ICache<>), typeof(Cache<>));
        services.TryAddSingleton<ICacheFactory, CacheFactory>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = new ConfigurationOptions
            {
                EndPoints =
                {
                    cacheOptions.RedisUrl ?? throw new InvalidOperationException("RedisUrl was not specified in config")
                },
                Password = cacheOptions.RedisPassword,
                Ssl = cacheOptions.RedisSsl,
                DefaultDatabase = cacheOptions.RedisDefaultDatabase
            };
        });

        var healthChecksBuilder = services.AddHealthChecks();
        healthChecksBuilder.AddCacheHealthCheck(cacheOptions);

        return services;
    }

    public static IHealthChecksBuilder AddCacheHealthCheck(this IHealthChecksBuilder healthChecksBuilder,
        CacheOptions cacheOptions)
    {
        var redisConnectionString =
            $"{cacheOptions.RedisUrl},ssl={cacheOptions.RedisSsl},password={cacheOptions.RedisPassword},defaultDatabase={cacheOptions.RedisDefaultDatabase}";

        healthChecksBuilder
            .AddRedis(redisConnectionString,
                name: "RedisCache-check",
                tags: RedisTags,
                failureStatus: HealthStatus.Degraded);

        return healthChecksBuilder;
    }
}