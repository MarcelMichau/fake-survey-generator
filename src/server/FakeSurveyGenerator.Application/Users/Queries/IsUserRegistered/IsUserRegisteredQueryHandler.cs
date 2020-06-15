using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Users.Queries.IsUserRegistered
{
    public sealed class IsUserRegisteredQueryHandler : IRequestHandler<IsUserRegisteredQuery, Result<bool>>
    {
        private readonly ISurveyContext _surveyContext;

        public IsUserRegisteredQueryHandler(ISurveyContext context)
        {
            _surveyContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Result<bool>> Handle(IsUserRegisteredQuery request, CancellationToken cancellationToken)
        {
            var isUserRegistered =
                await _surveyContext.Users.AnyAsync(user => user.ExternalUserId == request.UserId, cancellationToken);

            return Result.Success(isUserRegistered);
        }
    }
}