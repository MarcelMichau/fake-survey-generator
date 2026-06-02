using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Shared.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace FakeSurveyGenerator.Application.Features.Surveys;

public sealed record DeleteSurveyCommand(int Id) : ICommand<Result<int, Error>>;

public sealed class DeleteSurveyCommandValidator : AbstractValidator<DeleteSurveyCommand>
{
    public DeleteSurveyCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
    }
}

public sealed class DeleteSurveyCommandHandler(
    SurveyContext surveyContext,
    IUserService userService,
    HybridCache cache,
    IValidator<DeleteSurveyCommand> validator)
    : ICommandHandler<DeleteSurveyCommand, Result<int, Error>>
{
    private readonly SurveyContext _surveyContext =
        surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));

    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

    private readonly HybridCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));

    private readonly IValidator<DeleteSurveyCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    internal static string SurveyKey(int id) => $"survey:{id}";

    public async Task<Result<int, Error>> Handle(DeleteSurveyCommand request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Errors.General.ValidationError(validationResult);
        }

        var userInfo = await _userService.GetUserInfo(cancellationToken);

        var currentUserId = await _surveyContext.Users
            .Where(user => user.ExternalUserId == userInfo.Id)
            .Select(user => user.Id)
            .FirstAsync(cancellationToken);

        var surveyToDelete = await _surveyContext.Surveys
            .Where(survey => survey.Id == request.Id)
            .Select(survey => new
            {
                Survey = survey,
                OwnerId = EF.Property<int>(survey, "OwnerId")
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (surveyToDelete is null)
            return Errors.General.NotFound(nameof(Survey), request.Id);

        if (surveyToDelete.OwnerId != currentUserId)
            return Errors.General.Forbidden();

        _surveyContext.Surveys.Remove(surveyToDelete.Survey);
        await _surveyContext.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(SurveyKey(request.Id), cancellationToken);

        return request.Id;
    }
}
