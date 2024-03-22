namespace FakeSurveyGenerator.Api.Configuration;

internal static class DaprConfigurationExtensions
{
    public static IHostApplicationBuilder AddDaprConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDaprClient();

        return builder;
    }
}
