using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Users;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Identity;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Features.Users;

// This command has no properties as all the data needed to register a user is retrieved from the request context.
public sealed record RegisterUserCommand : ICommand<RegisterUserResult>;

public sealed record RegisterUserResult
{
    public required UserModel User { get; init; }
    public required bool IsNewRegistration { get; init; }
}

public sealed class RegisterUserCommandHandler(
    IUserService userService,
    SurveyContext surveyContext)
    : ICommandHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly SurveyContext _surveyContext =
        surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));

    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request,
        CancellationToken cancellationToken = default)
    {
        var userInfo = await _userService.GetUserInfo(cancellationToken);

        var existingUser = await _surveyContext.Users
            .FirstOrDefaultAsync(user => user.ExternalUserId == userInfo.Id, cancellationToken);
        if (existingUser is not null)
        {
            return new RegisterUserResult
            {
                User = existingUser.MapToModel(),
                IsNewRegistration = false
            };
        }

        var newUser = new User(NonEmptyString.Create(userInfo.DisplayName),
            NonEmptyString.Create(userInfo.EmailAddress), NonEmptyString.Create(userInfo.Id));

        await _surveyContext.Users.AddAsync(newUser, cancellationToken);

        await _surveyContext.SaveChangesAsync(cancellationToken);

        return new RegisterUserResult
        {
            User = newUser.MapToModel(),
            IsNewRegistration = true
        };
    }
}