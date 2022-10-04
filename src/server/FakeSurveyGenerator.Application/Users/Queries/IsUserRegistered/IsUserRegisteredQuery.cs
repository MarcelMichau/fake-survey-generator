using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Users.Queries.IsUserRegistered;

public sealed record IsUserRegisteredQuery(string UserId) : IRequest<Result<UserRegistrationStatusModel>>;

public sealed class IsUserRegisteredQueryHandler : IRequestHandler<IsUserRegisteredQuery, Result<UserRegistrationStatusModel>>
{
    private readonly ISurveyContext _surveyContext;

    public IsUserRegisteredQueryHandler(ISurveyContext context)
    {
        _surveyContext = context ?? throw new ArgumentNullException(nameof(context));
    }

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