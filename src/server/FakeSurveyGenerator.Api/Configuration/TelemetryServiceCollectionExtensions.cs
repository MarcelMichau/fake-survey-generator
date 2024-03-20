using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class TelemetryServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddTelemetryConfiguration(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetValue<string>("APPLICATIONINSIGHTS_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
            return builder;

        builder.Services.AddOpenTelemetry().UseAzureMonitor();

        return builder;
    }
}