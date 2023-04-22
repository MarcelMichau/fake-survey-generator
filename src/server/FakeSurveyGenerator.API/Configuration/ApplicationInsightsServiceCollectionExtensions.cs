namespace FakeSurveyGenerator.API.Configuration;

internal static class ApplicationInsightsServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationInsightsConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("APPLICATIONINSIGHTS_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
            return services;

        services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = connectionString;
        });

        return services;
    }
}