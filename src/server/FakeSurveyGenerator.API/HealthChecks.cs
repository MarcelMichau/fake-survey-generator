using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                .AddRedis(Environment.GetEnvironmentVariable("REDIS_URL"), "RedisCache-check",
                    tags: new[] {"redis-cache"},
                    failureStatus: HealthStatus.Degraded);

            //services.AddHealthChecksUI();

            return services;
        }
    }
}