using System.Reflection;
using FakeSurveyGenerator.Application.Infrastructure.Caching;
using FakeSurveyGenerator.Application.Infrastructure.DomainEvents;
using FakeSurveyGenerator.Application.Infrastructure.Identity;
using FakeSurveyGenerator.Application.Infrastructure.Notifications;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Behaviours;
using FakeSurveyGenerator.Application.Shared.DomainEvents;
using FakeSurveyGenerator.Application.Shared.Identity;
using FakeSurveyGenerator.Application.Shared.Notifications;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.Application;
public static class ServiceCollectionConfiguration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
        });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }

    private static IServiceCollection AddBaseInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<INotificationService, NotificationService>();

        services.AddDatabaseConfiguration(configuration);
        services.AddCacheConfiguration(configuration.GetSection(CacheOptions.Cache).Get<CacheOptions>() ?? throw new InvalidOperationException("Cache config section not found"));

        services.AddScoped<IDomainEventService, DomainEventService>();

        return services;
    }

    // There are two different extension methods to add the Infrastructure dependencies to the service collection.
    // This is because the services for OAuthUserInfo depend on a Token Provider which, in turn, depend on an HttpContext.
    // The AddInfrastructure method registers a SystemUserInfoService - this is intended to be used by long-running worker processes which do not have an HttpContext.
    // The AddInfrastructureForApi method registers an OAuthUserInfoService to get the current user info from an OAuth Identity Provider using the Access Token from the HTTP request - this is intended
    // to be used by APIs which do have an HttpContext & which have an ITokenProvider implementation registered with the service collection.

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBaseInfrastructure(configuration);
        services.AddSingleton<IUserService, SystemUserInfoService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureForApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBaseInfrastructure(configuration);
        services.AddOAuthConfiguration();

        return services;
    }
}
