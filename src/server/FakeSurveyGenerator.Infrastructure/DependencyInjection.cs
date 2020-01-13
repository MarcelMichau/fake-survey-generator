using FakeSurveyGenerator.Application.Common.Interfaces;
using FakeSurveyGenerator.Infrastructure.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificationService, NotificationService>();

            services.AddDatabaseConfiguration(configuration);
            services.AddCacheConfiguration(configuration);
            services.AddRepositoryConfiguration();

            return services;
        }
    }
}
