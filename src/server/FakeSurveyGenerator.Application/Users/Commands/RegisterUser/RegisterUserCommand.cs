using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Users.Models;
using MediatR;

namespace FakeSurveyGenerator.Application.Users.Commands.RegisterUser
{
    public sealed class RegisterUserCommand : IRequest<Result<UserModel, Error>>
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }

        public RegisterUserCommand()
        {
            
        }

        public RegisterUserCommand(string email, string name, string userId) : this()
        {
            Email = email;
            Name = name;
            UserId = userId;
        }
    }
}
