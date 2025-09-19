using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.DomainEvents;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.Infrastructure.Caching;
using FakeSurveyGenerator.Application.Infrastructure.Identity;
using FakeSurveyGenerator.Application.Infrastructure.Notifications;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Identity;
using FakeSurveyGenerator.Application.Shared.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using FakeSurveyGenerator.Application.Shared.Errors;
using FluentValidation;

namespace FakeSurveyGenerator.Application;

public static class HostApplicationBuilderConfiguration
{
    public static IHostApplicationBuilder AddApplication(this IHostApplicationBuilder builder)
    {
        // Command Handlers
        builder.Services
            .AddCommandHandler<CreateSurveyCommand, Result<SurveyModel, Error>, CreateSurveyCommandHandler>()
            .AddCommandHandler<RegisterUserCommand, Result<UserModel, Error>, RegisterUserCommandHandler>();

        // Query Handlers
        builder.Services.AddQueryHandler<GetUserSurveysQuery, Result<List<UserSurveyModel>, Error>, GetUserSurveysQueryHandler>()
            .AddQueryHandler<GetSurveyDetailQuery, Result<SurveyModel, Error>, GetSurveyDetailQueryHandler>()
            .AddQueryHandler<GetUserQuery, Result<UserModel, Error>, GetUserQueryHandler>()
            .AddQueryHandler<IsUserRegisteredQuery, Result<UserRegistrationStatusModel, Error>, IsUserRegisteredQueryHandler>();

        // Domain Event Handlers
        builder.Services
            .AddDomainEventHandler<SurveyCreatedDomainEvent, SendNotificationWhenSurveyCreatedDomainEventHandler>();

        // Validators
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Domain Event Publisher
        builder.Services.AddScoped<IEventBus, DomainEventPublisher>();

        return builder;
    }

    private static IServiceCollection AddCommandHandler<TCommand, TResult, THandler>(this IServiceCollection services)
        where TCommand : ICommand<TResult>
        where THandler : class, ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<ICommandHandler<TCommand, TResult>, THandler>();
        return services;
    }

    private static IServiceCollection AddQueryHandler<TQuery, TResult, THandler>(this IServiceCollection services)
        where TQuery : IQuery<TResult>
        where THandler : class, IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<IQueryHandler<TQuery, TResult>, THandler>();
        return services;
    }

    private static IServiceCollection AddDomainEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : IDomainEvent
        where THandler : class, IDomainEventHandler<TEvent>
    {
        services.AddScoped<IDomainEventHandler<TEvent>, THandler>();
        return services;
    }

    private static IHostApplicationBuilder AddBaseInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddScoped<INotificationService, NotificationService>();

        builder.AddDatabaseConfiguration();
        builder.AddCacheConfiguration();

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