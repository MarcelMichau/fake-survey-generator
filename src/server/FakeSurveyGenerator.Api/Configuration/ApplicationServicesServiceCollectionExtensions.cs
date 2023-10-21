using FakeSurveyGenerator.Api.Data;
using FakeSurveyGenerator.Application;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class ApplicationServicesServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServicesConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructureForApi(configuration);
        services.AddApplication();

        services.AddHostedService<DatabaseCreationHostedService>();

        return services;
    }
}