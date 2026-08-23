using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.Application.Infrastructure.Caching;

internal static class CacheConfigurationExtensions
{
    public static IHostApplicationBuilder AddCacheConfiguration(this IHostApplicationBuilder builder)
    {
        var hostName = builder.Configuration["Redis:HostName"];
        var password = builder.Configuration["Redis:Password"];

        if (!string.IsNullOrWhiteSpace(hostName) || !string.IsNullOrWhiteSpace(password))
        {
            if (string.IsNullOrWhiteSpace(hostName) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Both Redis:HostName and Redis:Password must be configured together.");
            }

            builder.Configuration["ConnectionStrings:cache"] = $"{hostName}:6379,password={password},ssl=false,abortConnect=false";
        }

        builder.AddRedisClientBuilder("cache")
            .WithDistributedCache();

        builder.Services.AddHybridCache();

        return builder;
    }
}