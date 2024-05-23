using FakeSurveyGenerator.Application.Shared.Caching;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.Application.Infrastructure.Caching;

internal static class CacheConfigurationExtensions
{
    public static IHostApplicationBuilder AddCacheConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton(typeof(ICache<>), typeof(Cache<>));
        builder.Services.TryAddSingleton<ICacheFactory, CacheFactory>();

        builder.AddRedisDistributedCache("cache");

        return builder;
    }
}