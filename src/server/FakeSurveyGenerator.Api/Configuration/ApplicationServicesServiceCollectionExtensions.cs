using FakeSurveyGenerator.Api.Data;
using FakeSurveyGenerator.Application;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class ApplicationServicesServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddApplicationServicesConfiguration(this IHostApplicationBuilder builder)
    {
        builder.AddInfrastructureForApi();
        builder.Services.AddApplication();

        builder.Services.AddHostedService<DatabaseCreationHostedService>();

        return builder;
    }
}