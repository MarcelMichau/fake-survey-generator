using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace FakeSurveyGenerator.Api.Configuration.HealthChecks;

internal static class HealthChecksConfigurationExtensions
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder UseHealthChecksConfiguration()
        {
            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
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