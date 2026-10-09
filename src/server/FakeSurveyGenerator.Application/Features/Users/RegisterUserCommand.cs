using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Users;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Shared.Identity;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Features.Users;

// This command has no properties as all the data needed to register a user is retrieved from the request context.
public sealed record RegisterUserCommand : ICommand<Result<RegisterUserResult, Error>>;

public sealed record RegisterUserResult
{
    public required UserModel User { get; init; }
    public required bool IsNewRegistration { get; init; }
}

public sealed class RegisterUserCommandHandler(
    IUserService userService,
    SurveyContext surveyContext)
    : ICommandHandler<RegisterUserCommand, Result<RegisterUserResult, Error>>
{
    private readonly SurveyContext _surveyContext =
        surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));

    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

    public async Task<Result<RegisterUserResult, Error>> Handle(RegisterUserCommand request,
        CancellationToken cancellationToken = default)
    {
        var userInfo = await _userService.GetUserInfo(cancellationToken);

        var existingUser = await FindUser(userInfo.Id, cancellationToken);
        if (existingUser is not null)
            return ExistingRegistration(existingUser);

        if (string.IsNullOrWhiteSpace(userInfo.Id) ||
            string.IsNullOrWhiteSpace(userInfo.DisplayName) ||
            string.IsNullOrWhiteSpace(userInfo.EmailAddress))
            return Errors.General.UserProfileIncomplete();

        var newUser = new User(NonEmptyString.Create(userInfo.DisplayName),
            NonEmptyString.Create(userInfo.EmailAddress), NonEmptyString.Create(userInfo.Id));

        await _surveyContext.Users.AddAsync(newUser, cancellationToken);

        try
        {
            await _surveyContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // A concurrent request may have registered the same user between the lookup above and the insert
            // (ExternalUserId is unique). Registration is idempotent, so return the winning registration.
            _surveyContext.Entry(newUser).State = EntityState.Detached;

            var concurrentlyRegisteredUser = await FindUser(userInfo.Id, cancellationToken);
            if (concurrentlyRegisteredUser is null)
                throw;

            return ExistingRegistration(concurrentlyRegisteredUser);
        }

        return new RegisterUserResult
        {
            User = newUser.MapToModel(),
            IsNewRegistration = true
        };
    }

    private Task<User?> FindUser(string externalUserId, CancellationToken cancellationToken)
    {
        return _surveyContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.ExternalUserId == externalUserId, cancellationToken);
    }

    private static RegisterUserResult ExistingRegistration(User user)
    {
        return new RegisterUserResult
        {
            User = user.MapToModel(),
            IsNewRegistration = false
        };
    }
}
