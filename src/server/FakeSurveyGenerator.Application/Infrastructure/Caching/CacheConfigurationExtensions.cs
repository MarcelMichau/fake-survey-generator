using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.Application.Infrastructure.Caching;

internal static class CacheConfigurationExtensions
{
    public static IHostApplicationBuilder AddCacheConfiguration(this IHostApplicationBuilder builder)
    {
        builder.AddRedisClientBuilder("cache")
            .WithAzureAuthentication()
            .WithDistributedCache();

        builder.Services.AddHybridCache();

        return builder;
    }
}