using FakeSurveyGenerator.Application.Common.DomainEvents;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Common.Notifications;
using FakeSurveyGenerator.Infrastructure.Caching;
using FakeSurveyGenerator.Infrastructure.DomainEvents;
using FakeSurveyGenerator.Infrastructure.Identity;
using FakeSurveyGenerator.Infrastructure.Notifications;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.Infrastructure;

public static class DependencyInjection
{
    // There are two different extension methods to add the Infrastructure dependencies to the service collection.
    // This is because the services for OAuthUserInfo depend on a Token Provider which, in turn, depend on an HttpContext.
    // The AddInfrastructure method registers a SystemUserInfoService - this is intended to be used by long-running worker processes which do not have an HttpContext.
    // The AddInfrastructureForApi method registers an OAuthUserInfoService to get the current user info from an OAuth Identity Provider using the Access Token from the HTTP request - this is intended
    // to be used by APIs which do have an HttpContext & which have an ITokenProvider implementation registered with the service collection.

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<INotificationService, NotificationService>();

        services.AddDatabaseConfiguration(configuration);
        services.AddCacheConfiguration(configuration);
        services.AddSingleton<IUserService, SystemUserInfoService>();
        services.AddScoped<IDomainEventService, DomainEventService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureForApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<INotificationService, NotificationService>();

        services.AddDatabaseConfiguration(configuration);
        services.AddCacheConfiguration(configuration);
        services.AddOAuthConfiguration();
        services.AddScoped<IDomainEventService, DomainEventService>();

        return services;
    }
}