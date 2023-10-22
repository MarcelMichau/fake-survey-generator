using FakeSurveyGenerator.Application.Shared.Identity;

namespace FakeSurveyGenerator.Application.Infrastructure.Identity;

internal sealed class SystemUserInfoService : IUserService
{
    public string GetUserIdentity() => new SystemUser().Id;

    public Task<IUser> GetUserInfo(CancellationToken cancellationToken = default) => Task.FromResult<IUser>(new SystemUser());
}