using System;
using FakeSurveyGenerator.Infrastructure.Persistence;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FakeSurveyGenerator.API.Configuration.HealthChecks
{
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
                $"{configuration.GetValue<string>("REDIS_URL")},ssl={configuration.GetValue<string>("REDIS_SSL")},password={configuration.GetValue<string>("REDIS_PASSWORD")},defaultDatabase={configuration.GetValue<string>("REDIS_DEFAULT_DATABASE")}";

            healthChecksBuilder
                .AddRedis(redisConnectionString,
                    "RedisCache-check",
                    tags: new[] {"redis-cache", "ready"},
                    failureStatus: HealthStatus.Degraded);

            healthChecksBuilder.AddIdentityServer(
                new Uri($"{configuration.GetValue<string>("IDENTITY_PROVIDER_URL")}"),
                "IdentityProvider-check",
                tags: new[] {"identity-provider", "ready"},
                failureStatus: HealthStatus.Unhealthy,
                timeout: new TimeSpan(0,0,5));

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
}