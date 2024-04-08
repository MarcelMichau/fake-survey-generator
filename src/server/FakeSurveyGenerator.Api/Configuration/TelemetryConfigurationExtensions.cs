using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class TelemetryConfigurationExtensions
{
    public static IHostApplicationBuilder AddTelemetryConfiguration(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetValue<string>("APPLICATIONINSIGHTS_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
            return builder;

        builder.Services.AddOpenTelemetry().UseAzureMonitor();

        var resourceAttributes = new Dictionary<string, object>
        {
            {"service.name", "fake-survey-generator-api"},
            {"service.namespace", "fake-survey-generator"},
            {"service.instance.id", builder.Environment.IsDevelopment() ? "development" : "production"}
        };

        builder.Services.ConfigureOpenTelemetryTracerProvider((_, tracerProviderBuilder) =>
            tracerProviderBuilder.ConfigureResource(resourceBuilder =>
                resourceBuilder.AddAttributes(resourceAttributes)));

        return builder;
    }
}