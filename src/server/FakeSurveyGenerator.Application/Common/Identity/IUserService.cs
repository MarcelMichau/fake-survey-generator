using System.Threading;
using System.Threading.Tasks;

namespace FakeSurveyGenerator.Application.Common.Identity;

public interface IUserService
{
    string GetUserIdentity();
    Task<IUser> GetUserInfo(CancellationToken cancellationToken = default);
}