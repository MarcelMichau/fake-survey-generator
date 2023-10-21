using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Features.Users;
public sealed record IsUserRegisteredQuery(string UserId) : IRequest<Result<UserRegistrationStatusModel>>;

public sealed class IsUserRegisteredQueryHandler(SurveyContext context) : IRequestHandler<IsUserRegisteredQuery, Result<UserRegistrationStatusModel>>
{
    private readonly SurveyContext _surveyContext = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Result<UserRegistrationStatusModel>> Handle(IsUserRegisteredQuery request, CancellationToken cancellationToken)
    {
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
