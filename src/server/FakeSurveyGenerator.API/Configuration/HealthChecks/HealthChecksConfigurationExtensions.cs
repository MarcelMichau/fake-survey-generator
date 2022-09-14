using FakeSurveyGenerator.Infrastructure.Persistence;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FakeSurveyGenerator.API.Configuration.HealthChecks;

internal static class HealthChecksConfigurationExtensions
{
    public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        healthChecksBuilder
            .AddSqlServer(
                configuration.GetConnectionString(nameof(SurveyContext)),
                name: "FakeSurveyGeneratorDB-check",
                tags: new[] { "fake-survey-generator-db", "ready" },
                failureStatus: HealthStatus.Unhealthy);

        var redisConnectionString =
            $"{configuration.GetValue<string>("Cache:RedisUrl")},ssl={configuration.GetValue<string>("Cache:RedisSsl")},password={configuration.GetValue<string>("Cache:RedisPassword")},defaultDatabase={configuration.GetValue<string>("Cache:RedisDefaultDatabase")}";

        healthChecksBuilder
            .AddRedis(redisConnectionString,
                "RedisCache-check",
                tags: new[] { "redis-cache", "ready" },
                failureStatus: HealthStatus.Degraded);

        healthChecksBuilder.AddIdentityServer(
            new Uri($"{configuration.GetValue<string>("IDENTITY_PROVIDER_URL")}"),
            "IdentityProvider-check",
            tags: new[] { "identity-provider", "ready" },
            failureStatus: HealthStatus.Unhealthy,
            timeout: new TimeSpan(0, 0, 5));

        return services;
    }

    public static IEndpointRouteBuilder UseHealthChecksConfiguration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        return endpoints;
    }
}