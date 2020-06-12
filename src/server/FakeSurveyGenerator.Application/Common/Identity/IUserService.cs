using System.Threading;
using System.Threading.Tasks;

namespace FakeSurveyGenerator.Application.Common.Identity
{
    public interface IUserService
    {
        Task<IUser> GetUserInfo(string accessToken, CancellationToken cancellationToken = default);
    }
}
