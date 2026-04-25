using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Caching;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Shared.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

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
    ICache<SurveyModel?> cache,
    IValidator<DeleteSurveyCommand> validator)
    : ICommandHandler<DeleteSurveyCommand, Result<int, Error>>
{
    private readonly SurveyContext _surveyContext =
        surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));

    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

    private readonly ICache<SurveyModel?> _cache = cache ?? throw new ArgumentNullException(nameof(cache));

    private readonly IValidator<DeleteSurveyCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<Result<int, Error>> Handle(DeleteSurveyCommand request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Errors.General.ValidationError(validationResult);
        }

        var survey = await _surveyContext.Surveys
            .Include(s => s.Owner)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (survey is null)
            return Errors.General.NotFound(nameof(Survey), request.Id);

        var userInfo = await _userService.GetUserInfo(cancellationToken);

        var currentUser = await _surveyContext.Users
            .FirstAsync(user => user.ExternalUserId == userInfo.Id, cancellationToken);

        if (survey.Owner.Id != currentUser.Id)
            return Errors.General.Forbidden();

        _surveyContext.Surveys.Remove(survey);

        await _surveyContext.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(request.Id.ToString(), cancellationToken);

        return request.Id;
    }
}
