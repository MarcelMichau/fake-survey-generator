using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FakeSurveyGenerator.Api.Configuration.HealthChecks;

internal static class HealthChecksConfigurationExtensions
{
    internal static readonly string[] DbTags = { "fake-survey-generator-db", "ready" };
    internal static readonly string[] RedisTags = { "redis-cache", "ready" };
    internal static readonly string[] IdentityProviderTags = { "identity-provider", "ready" };

    public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        healthChecksBuilder
            .AddSqlServer(
                configuration.GetConnectionString(nameof(SurveyContext)) ?? throw new InvalidOperationException($"Connection String for {nameof(SurveyContext)} not found in configuration"),
                name: "FakeSurveyGeneratorDB-check",
                tags: DbTags,
                failureStatus: HealthStatus.Unhealthy);

        var redisConnectionString =
            $"{configuration.GetValue<string>("Cache:RedisUrl")},ssl={configuration.GetValue<string>("Cache:RedisSsl")},password={configuration.GetValue<string>("Cache:RedisPassword")},defaultDatabase={configuration.GetValue<string>("Cache:RedisDefaultDatabase")}";

        healthChecksBuilder
            .AddRedis(redisConnectionString,
                name:"RedisCache-check",
                tags: RedisTags,
                failureStatus: HealthStatus.Degraded);

        healthChecksBuilder.AddIdentityServer(
            new Uri($"{configuration.GetValue<string>("IDENTITY_PROVIDER_URL")}"),
            name: "IdentityProvider-check",
            tags: IdentityProviderTags,
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