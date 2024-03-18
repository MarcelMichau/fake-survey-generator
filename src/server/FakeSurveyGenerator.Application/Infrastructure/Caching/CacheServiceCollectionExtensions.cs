using FakeSurveyGenerator.Application.Shared.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Aspire.StackExchange.Redis;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.Application.Infrastructure.Caching;

internal static class CacheServiceCollectionExtensions
{
    internal static readonly string[] RedisTags = ["redis-cache", "ready"];

    public static IHostApplicationBuilder AddCacheConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton(typeof(ICache<>), typeof(Cache<>));
        builder.Services.TryAddSingleton<ICacheFactory, CacheFactory>();

        builder.AddRedisDistributedCache("cache");

        //services.AddStackExchangeRedisCache(options =>
        //{
        //    options.ConfigurationOptions = new ConfigurationOptions
        //    {
        //        EndPoints =
        //        {
        //            cacheOptions.RedisUrl ?? throw new InvalidOperationException("RedisUrl was not specified in config")
        //        },
        //        Password = cacheOptions.RedisPassword,
        //        Ssl = cacheOptions.RedisSsl,
        //        DefaultDatabase = cacheOptions.RedisDefaultDatabase
        //    };
        //});

        //var healthChecksBuilder = services.AddHealthChecks();
        //healthChecksBuilder.AddCacheHealthCheck(cacheOptions);

        return builder;
    }

    //public static IHealthChecksBuilder AddCacheHealthCheck(this IHealthChecksBuilder healthChecksBuilder,
    //    CacheOptions cacheOptions)
    //{
    //    var redisConnectionString =
    //        $"{cacheOptions.RedisUrl},ssl={cacheOptions.RedisSsl},password={cacheOptions.RedisPassword},defaultDatabase={cacheOptions.RedisDefaultDatabase}";

    //    healthChecksBuilder
    //        .AddRedis(redisConnectionString,
    //            name: "RedisCache-check",
    //            tags: RedisTags,
    //            failureStatus: HealthStatus.Degraded);

    //    return healthChecksBuilder;
    //}
}