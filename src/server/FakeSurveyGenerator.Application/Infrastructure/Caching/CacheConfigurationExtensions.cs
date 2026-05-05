using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.Application.Infrastructure.Caching;

internal static class CacheConfigurationExtensions
{
    public static IHostApplicationBuilder AddCacheConfiguration(this IHostApplicationBuilder builder)
    {
        builder.AddRedisDistributedCache("cache");

        builder.Services.AddHybridCache();

        return builder;
    }
}