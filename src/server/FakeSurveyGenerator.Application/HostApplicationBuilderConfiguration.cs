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
using FakeSurveyGenerator.Application.Infrastructure.TypeSafe;
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
    extension(IServiceCollection services)
    {
        private IServiceCollection AddCommandHandler<TCommand, TResult, THandler>()
            where TCommand : ICommand<TResult>
            where THandler : class, ICommandHandler<TCommand, TResult>
        {
            services.AddScoped<ICommandHandler<TCommand, TResult>, THandler>();
            return services;
        }

        private IServiceCollection AddQueryHandler<TQuery, TResult, THandler>()
            where TQuery : IQuery<TResult>
            where THandler : class, IQueryHandler<TQuery, TResult>
        {
            services.AddScoped<IQueryHandler<TQuery, TResult>, THandler>();
            return services;
        }

        private IServiceCollection AddDomainEventHandler<TEvent, THandler>()
            where TEvent : IDomainEvent
            where THandler : class, IDomainEventHandler<TEvent>
        {
            services.AddScoped<IDomainEventHandler<TEvent>, THandler>();
            services.AddScoped<IDomainEventHandler>(provider =>
                provider.GetRequiredService<IDomainEventHandler<TEvent>>());
            return services;
        }
    }

    extension(IHostApplicationBuilder builder)
    {
        private IHostApplicationBuilder AddBaseInfrastructure()
        {
            builder.Services.AddSingleton(TimeProvider.System);

            builder.Services.AddScoped<INotificationService, NotificationService>();

            builder.AddDatabaseConfiguration();
            builder.AddCacheConfiguration();
            builder.AddTypeSafeConfiguration();

            return builder;
        }

        // There are two different extension methods to add the Infrastructure dependencies to the service collection.
        // This is because the services for OAuthUserInfo depend on a Token Provider which, in turn, depend on an HttpContext.
        // The AddInfrastructure method registers a SystemUserInfoService - this is intended to be used by long-running worker processes which do not have an HttpContext.
        // The AddInfrastructureForApi method registers an OAuthUserInfoService to get the current user info from an OAuth Identity Provider using the Access Token from the HTTP request - this is intended
        // to be used by APIs which do have an HttpContext & which have an ITokenProvider implementation registered with the service collection.
        public IHostApplicationBuilder AddInfrastructure()
        {
            builder.AddBaseInfrastructure();
            builder.Services.AddSingleton<IUserService, SystemUserInfoService>();

            return builder;
        }

        public IHostApplicationBuilder AddInfrastructureForApi()
        {
            builder.AddBaseInfrastructure();
            builder.AddOAuthConfiguration();

            return builder;
        }

        public IHostApplicationBuilder AddApplication()
        {
            // Command Handlers
            builder.Services
                .AddCommandHandler<CreateSurveyCommand, Result<SurveyModel, Error>, CreateSurveyCommandHandler>()
                .AddCommandHandler<AnalyzeSurveyCommand, Result<SurveyAnalysisModel, Error>, AnalyzeSurveyCommandHandler>()
                .AddCommandHandler<DeleteSurveyCommand, Result<int, Error>, DeleteSurveyCommandHandler>()
                .AddCommandHandler<RegisterUserCommand, Result<RegisterUserResult, Error>, RegisterUserCommandHandler>();

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
    }
}