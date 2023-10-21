using AutoMapper;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Users;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Shared.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Features.Users;

// This command has no properties as all the data needed to register a user is retrieved from the request context.
public sealed record RegisterUserCommand : IRequest<Result<UserModel, Error>>;

public sealed class RegisterUserCommandHandler(IUserService userService, SurveyContext surveyContext, IMapper mapper)
    : IRequestHandler<RegisterUserCommand, Result<UserModel, Error>>
{
    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    private readonly SurveyContext _surveyContext = surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public async Task<Result<UserModel, Error>> Handle(RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var userInfo = await _userService.GetUserInfo(cancellationToken);

        if (await _surveyContext.Users.AnyAsync(user => user.ExternalUserId == userInfo.Id, cancellationToken))
            return Errors.General.UserAlreadyRegistered(userInfo.Id);

        var newUser = new User(NonEmptyString.Create(userInfo.DisplayName),
            NonEmptyString.Create(userInfo.EmailAddress), NonEmptyString.Create(userInfo.Id));

        await _surveyContext.Users.AddAsync(newUser, cancellationToken);

        await _surveyContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserModel>(newUser);
    }
}
