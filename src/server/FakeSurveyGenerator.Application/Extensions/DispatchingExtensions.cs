using System.Reflection;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.EventBus;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.Application.Extensions;

public static class DispatchingExtensions
{
    public static IServiceCollection AddDispatching(this IServiceCollection services)
    {
        services.AddCommandHandler<CreateSurveyCommand, Result<SurveyModel, Error>, CreateSurveyCommandHandler>();
        services.AddCommandHandler<RegisterUserCommand, Result<UserModel, Error>, RegisterUserCommandHandler>();
        
        services.AddQueryHandler<GetUserSurveysQuery, Result<List<UserSurveyModel>, Error>, GetUserSurveysQueryHandler>();
        services.AddQueryHandler<GetSurveyDetailQuery, Result<SurveyModel, Error>, GetSurveyDetailQueryHandler>();
        services.AddQueryHandler<GetUserQuery, Result<UserModel, Error>, GetUserQueryHandler>();
        services.AddQueryHandler<IsUserRegisteredQuery, Result<UserRegistrationStatusModel, Error>, IsUserRegisteredQueryHandler>();
        
        services.AddDomainEventHandler<SurveyCreatedDomainEvent, SendNotificationWhenSurveyCreatedDomainEventHandler>();
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Register the in-process event bus
        services.AddScoped<IEventBus, InMemoryEventBus>();
        
        return services;
    }

    public static IServiceCollection AddCommandHandler<TCommand, TResult, THandler>(this IServiceCollection services)
        where TCommand : ICommand<TResult>
        where THandler : class, ICommandHandler<TCommand, TResult>
    {
        services.AddScoped<ICommandHandler<TCommand, TResult>, THandler>();
        return services;
    }

    public static IServiceCollection AddQueryHandler<TQuery, TResult, THandler>(this IServiceCollection services)
        where TQuery : IQuery<TResult>
        where THandler : class, IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<IQueryHandler<TQuery, TResult>, THandler>();
        return services;
    }

    public static IServiceCollection AddDomainEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : IDomainEvent
        where THandler : class, IDomainEventHandler<TEvent>
    {
        services.AddScoped<IDomainEventHandler<TEvent>, THandler>();
        return services;
    }
}