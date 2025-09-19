using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Domain.Users;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Errors;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Features.Users;

public sealed record GetUserQuery(int Id) : IQuery<Result<UserModel, Error>>;

public sealed class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be greater than 0");
    }
}

public sealed class GetUserQueryHandler(
    SurveyContext context,
    IValidator<GetUserQuery> validator) : IQueryHandler<GetUserQuery, Result<UserModel, Error>>
{
    private readonly SurveyContext _surveyContext = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IValidator<GetUserQuery> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<Result<UserModel, Error>> Handle(GetUserQuery request, CancellationToken cancellationToken = default)
    {
        // Validate the query first
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Errors.General.ValidationError(validationResult);
        }

        var user = await _surveyContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        return user is null ? Errors.General.NotFound(nameof(User), request.Id) : user.MapToModel();
    }
}