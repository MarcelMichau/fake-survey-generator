using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Users.Models;
using MediatR;

namespace FakeSurveyGenerator.Application.Users.Commands.RegisterUser
{
    public sealed class RegisterUserCommand : IRequest<Result<UserModel, Error>>
    {
        public RegisterUserCommand()
        {
            
        }
    }
}
