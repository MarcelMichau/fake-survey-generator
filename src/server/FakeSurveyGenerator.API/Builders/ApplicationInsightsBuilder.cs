using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Builders
{
    internal static class ApplicationInsightsBuilder
    {
        public static IServiceCollection AddApplicationInsightsConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            var instrumentationKey = configuration.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");

            if (string.IsNullOrWhiteSpace(instrumentationKey))
                return services;

            services.AddApplicationInsightsTelemetry(instrumentationKey);

            return services;
        }
    }
}
