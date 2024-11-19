using FakeSurveyGenerator.Application.Shared.Caching;
using Microsoft.Extensions.DependencyInjection;
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

#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        return builder;
    }
}