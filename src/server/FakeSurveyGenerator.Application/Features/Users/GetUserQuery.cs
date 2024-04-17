using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Domain.Users;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Features.Users;
public sealed record GetUserQuery(int Id) : IRequest<Result<UserModel, Error>>;

public sealed class GetUserQueryHandler(SurveyContext context) : IRequestHandler<GetUserQuery, Result<UserModel, Error>>
{
    private readonly SurveyContext _surveyContext = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Result<UserModel, Error>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _surveyContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        return user is null ? Errors.General.NotFound(nameof(User), request.Id) : user.MapToModel();
    }
}
