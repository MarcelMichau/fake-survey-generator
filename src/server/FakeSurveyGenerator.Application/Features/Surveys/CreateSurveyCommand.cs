using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Features.Notifications;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Shared.Identity;
using FakeSurveyGenerator.Application.Shared.Notifications;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Features.Surveys;

public sealed record CreateSurveyCommand() : IRequest<Result<SurveyModel, Error>>
{
    public required string SurveyTopic { get; init; }

    public required int NumberOfRespondents { get; init; }

    public required string RespondentType { get; init; }

    public required IEnumerable<SurveyOptionDto> SurveyOptions { get; init; } = [];
}

public sealed record SurveyOptionDto
{
    public required string OptionText { get; init; }
    public int PreferredNumberOfVotes { get; init; }
}

public sealed class CreateSurveyCommandValidator : AbstractValidator<CreateSurveyCommand>
{
    public CreateSurveyCommandValidator()
    {
        RuleFor(command => command.SurveyTopic)
            .MaximumLength(250)
            .NotEmpty();

        RuleFor(command => command.RespondentType)
            .MaximumLength(250)
            .NotEmpty();

        RuleFor(command => command.NumberOfRespondents)
            .GreaterThan(0);

        RuleFor(command => command.SurveyOptions)
            .NotEmpty();

        RuleForEach(command => command.SurveyOptions)
            .SetValidator(new SurveyOptionValidator());
    }
}

public sealed class SurveyOptionValidator : AbstractValidator<SurveyOptionDto>
{
    public SurveyOptionValidator()
    {
        RuleFor(command => command.OptionText)
            .MaximumLength(250)
            .NotEmpty();
    }
}

public sealed class CreateSurveyCommandHandler(
    SurveyContext surveyContext,
    IUserService userService)
    : IRequestHandler<CreateSurveyCommand, Result<SurveyModel, Error>>
{
    private readonly SurveyContext _surveyContext =
        surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));

    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

    public async Task<Result<SurveyModel, Error>> Handle(CreateSurveyCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userInfo = await _userService.GetUserInfo(cancellationToken);

            var surveyOwner =
                await _surveyContext.Users.FirstAsync(user => user.ExternalUserId == userInfo.Id,
                    cancellationToken);

            var survey = new Survey(surveyOwner, NonEmptyString.Create(request.SurveyTopic),
                request.NumberOfRespondents, NonEmptyString.Create(request.RespondentType));

            survey.AddSurveyOptions(request.SurveyOptions.Select(option =>
                new SurveyOption(NonEmptyString.Create(option.OptionText), option.PreferredNumberOfVotes)));

            survey.CalculateOutcome();

            await _surveyContext.Surveys.AddAsync(survey, cancellationToken);

            await _surveyContext.SaveChangesAsync(cancellationToken);

            return survey.MapToModel();
        }
        catch (SurveyDomainException e)
        {
            return new Error("survey.domain.exception", e.Message);
        }
    }
}

public sealed class
    SendNotificationWhenSurveyCreatedDomainEventHandler(INotificationService notificationService)
    : INotificationHandler<SurveyCreatedDomainEvent>
{
    public async Task Handle(SurveyCreatedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        await notificationService.SendMessage(
            new MessageModel("System", "Whom It May Concern", "New Survey Created",
                $"Survey with ID: {notification.Survey.Id} created"), cancellationToken);
    }
}