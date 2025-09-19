using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Errors;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Features.Users;

public sealed record IsUserRegisteredQuery(string UserId) : IQuery<Result<UserRegistrationStatusModel, Error>>;

public sealed class IsUserRegisteredQueryValidator : AbstractValidator<IsUserRegisteredQuery>
{
    public IsUserRegisteredQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
    }
}

public sealed class IsUserRegisteredQueryHandler(
    SurveyContext context,
    IValidator<IsUserRegisteredQuery> validator)
    : IQueryHandler<IsUserRegisteredQuery, Result<UserRegistrationStatusModel, Error>>
{
    private readonly SurveyContext _surveyContext = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IValidator<IsUserRegisteredQuery> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<Result<UserRegistrationStatusModel, Error>> Handle(IsUserRegisteredQuery request,
        CancellationToken cancellationToken = default)
    {
        // Validate the query first
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Errors.General.ValidationError(validationResult);
        }

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