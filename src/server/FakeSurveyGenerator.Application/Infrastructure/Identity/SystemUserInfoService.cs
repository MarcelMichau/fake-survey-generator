using FakeSurveyGenerator.Application.Shared.Identity;

namespace FakeSurveyGenerator.Application.Infrastructure.Identity;

internal sealed class SystemUserInfoService : IUserService
{
    public string GetUserIdentity()
    {
        return new SystemUser().Id;
    }

    public Task<IUser> GetUserInfo(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IUser>(new SystemUser());
    }
}