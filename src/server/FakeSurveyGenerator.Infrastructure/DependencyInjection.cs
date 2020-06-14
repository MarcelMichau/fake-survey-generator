using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Common.Notifications;
using FakeSurveyGenerator.Infrastructure.Builders;
using FakeSurveyGenerator.Infrastructure.Identity;
using FakeSurveyGenerator.Infrastructure.Notifications;
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
            services.AddSingleton<IUserService, SystemUserInfoService>();

            return services;
        }

        public static IServiceCollection AddInfrastructureForApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<INotificationService, NotificationService>();

            services.AddDatabaseConfiguration(configuration);
            services.AddCacheConfiguration(configuration);
            services.AddOAuthConfiguration();

            return services;
        }
    }
}
