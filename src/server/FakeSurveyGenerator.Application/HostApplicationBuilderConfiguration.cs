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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.Application;

public static class HostApplicationBuilderConfiguration
{
    public static IHostApplicationBuilder AddApplication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
        });

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return builder;
    }

    private static IHostApplicationBuilder AddBaseInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddScoped<INotificationService, NotificationService>();

        builder.AddDatabaseConfiguration(builder.Configuration);
        builder.AddCacheConfiguration();

        builder.Services.AddScoped<IDomainEventService, DomainEventService>();

        return builder;
    }

    // There are two different extension methods to add the Infrastructure dependencies to the service collection.
    // This is because the services for OAuthUserInfo depend on a Token Provider which, in turn, depend on an HttpContext.
    // The AddInfrastructure method registers a SystemUserInfoService - this is intended to be used by long-running worker processes which do not have an HttpContext.
    // The AddInfrastructureForApi method registers an OAuthUserInfoService to get the current user info from an OAuth Identity Provider using the Access Token from the HTTP request - this is intended
    // to be used by APIs which do have an HttpContext & which have an ITokenProvider implementation registered with the service collection.

    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddBaseInfrastructure();
        builder.Services.AddSingleton<IUserService, SystemUserInfoService>();

        return builder;
    }

    public static IHostApplicationBuilder AddInfrastructureForApi(this IHostApplicationBuilder builder)
    {
        builder.AddBaseInfrastructure();
        builder.AddOAuthConfiguration();

        return builder;
    }
}