using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Users.Models;
using MediatR;

namespace FakeSurveyGenerator.Application.Users.Queries.GetUser
{
    public sealed class GetUserQuery : IRequest<Result<UserModel, Error>>
    {
        public int Id { get; }

        public GetUserQuery(int id)
        {
            Id = id;
        }
    }
}
