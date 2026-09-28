using FakeSurveyGenerator.Api.Data;
using FakeSurveyGenerator.Application;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class ApplicationServicesConfigurationExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddApplicationServicesConfiguration()
        {
            builder.AddInfrastructureForApi();
            builder.AddApplication();

            builder.Services.AddHostedService<DatabaseCreationHostedService>();

            return builder;
        }
    }
}