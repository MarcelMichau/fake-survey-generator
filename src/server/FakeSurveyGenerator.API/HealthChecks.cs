using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FakeSurveyGenerator.API
{
    public static class HealthChecks
    {
        public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services,
            IConfiguration configuration)
        {
            var healthChecksBuilder = services.AddHealthChecks();

            healthChecksBuilder
                .AddSqlServer(
                    configuration["ConnectionStrings:SurveyContext"],
                    name: "FakeSurveyGeneratorDB-check",
                    tags: new[] {"fake-survey-generator-db"},
                    failureStatus: HealthStatus.Unhealthy);

            healthChecksBuilder
                .AddRedis($"{Environment.GetEnvironmentVariable("REDIS_URL")},ssl={Environment.GetEnvironmentVariable("REDIS_SSL")},password={Environment.GetEnvironmentVariable("REDIS_PASSWORD")}", "RedisCache-check",
                    tags: new[] {"redis-cache"},
                    failureStatus: HealthStatus.Degraded);

            //services.AddHealthChecksUI();

            return services;
        }
    }
}