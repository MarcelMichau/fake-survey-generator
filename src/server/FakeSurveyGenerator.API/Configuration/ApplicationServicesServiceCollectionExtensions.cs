using FakeSurveyGenerator.API.HostedServices;
using FakeSurveyGenerator.Application;
using FakeSurveyGenerator.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Configuration
{
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
}
