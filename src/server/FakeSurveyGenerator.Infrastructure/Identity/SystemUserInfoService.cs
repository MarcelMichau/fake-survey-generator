using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Identity;

namespace FakeSurveyGenerator.Infrastructure.Identity;

internal sealed class SystemUserInfoService : IUserService
{
    public string GetUserIdentity() => new SystemUser().Id;

    public Task<IUser> GetUserInfo(CancellationToken cancellationToken = default) => Task.FromResult<IUser>(new SystemUser());
}