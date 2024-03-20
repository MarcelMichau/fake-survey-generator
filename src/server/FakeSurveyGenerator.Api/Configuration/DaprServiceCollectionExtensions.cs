namespace FakeSurveyGenerator.Api.Configuration;

internal static class DaprServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddDaprConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDaprClient();

        return builder;
    }
}
