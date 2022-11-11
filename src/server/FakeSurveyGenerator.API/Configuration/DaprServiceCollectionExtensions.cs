namespace FakeSurveyGenerator.API.Configuration;

internal static class DaprServiceCollectionExtensions
{
    public static IServiceCollection AddDaprConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDaprClient();

        return services;
    }
}
