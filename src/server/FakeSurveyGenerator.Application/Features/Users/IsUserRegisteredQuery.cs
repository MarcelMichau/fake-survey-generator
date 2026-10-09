using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Shared.Identity;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Features.Users;

public sealed record IsUserRegisteredQuery(string UserId) : IQuery<Result<UserRegistrationStatusModel, Error>>;

[UsedImplicitly]
public sealed class IsUserRegisteredQueryValidator : AbstractValidator<IsUserRegisteredQuery>
{
    public IsUserRegisteredQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
    }
}

public sealed class IsUserRegisteredQueryHandler(
    SurveyContext context,
    IUserService userService,
    IValidator<IsUserRegisteredQuery> validator)
    : IQueryHandler<IsUserRegisteredQuery, Result<UserRegistrationStatusModel, Error>>
{
    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    private readonly SurveyContext _surveyContext = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IValidator<IsUserRegisteredQuery> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<Result<UserRegistrationStatusModel, Error>> Handle(IsUserRegisteredQuery request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Errors.General.ValidationError(validationResult);
        }

        // Users may only check their own registration status, not probe for other users' accounts.
        var userInfo = await _userService.GetUserInfo(cancellationToken);
        if (request.UserId != userInfo.Id)
            return Errors.General.Forbidden();

        var isUserRegistered =
            await _surveyContext.Users.AsNoTracking()
                .AnyAsync(user => user.ExternalUserId == request.UserId, cancellationToken);

        return new UserRegistrationStatusModel
        {
            IsUserRegistered = isUserRegistered
        };
    }
}

public sealed class UserRegistrationStatusModel
{
    public bool IsUserRegistered { get; init; }
}