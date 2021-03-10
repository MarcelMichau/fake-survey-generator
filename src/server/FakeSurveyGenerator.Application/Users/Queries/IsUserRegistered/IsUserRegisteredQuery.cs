using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Users.Queries.IsUserRegistered
{
    public sealed record IsUserRegisteredQuery : IRequest<Result<UserRegistrationStatusModel>>
    {
        public string UserId { get; }

        public IsUserRegisteredQuery(string userId)
        {
            UserId = userId;
        }
    }

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

            return Result.Success(new UserRegistrationStatusModel
            {
                IsUserRegistered = isUserRegistered
            });
        }
    }
}
