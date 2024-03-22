using FakeSurveyGenerator.Api.Data;
using FakeSurveyGenerator.Application;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class ApplicationServicesConfigurationExtensions
{
    public static IHostApplicationBuilder AddApplicationServicesConfiguration(this IHostApplicationBuilder builder)
    {
        builder.AddInfrastructureForApi();
        builder.AddApplication();

        builder.Services.AddHostedService<DatabaseCreationHostedService>();

        return builder;
    }
}