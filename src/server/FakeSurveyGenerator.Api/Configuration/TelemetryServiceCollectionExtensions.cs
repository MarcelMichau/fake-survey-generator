using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class TelemetryServiceCollectionExtensions
{
    public static IServiceCollection AddTelemetryConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("APPLICATIONINSIGHTS_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
            return services;

        services.AddOpenTelemetry().UseAzureMonitor();

        return services;
    }
}