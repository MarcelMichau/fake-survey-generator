using CSharpFunctionalExtensions;
using MediatR;

namespace FakeSurveyGenerator.Application.Users.Queries.IsUserRegistered
{
    public sealed class IsUserRegisteredQuery : IRequest<Result<bool>>
    {
        public string UserId { get; set; }

        public IsUserRegisteredQuery(string userId)
        {
            UserId = userId;
        }
    }
}
