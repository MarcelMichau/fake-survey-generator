namespace FakeSurveyGenerator.Application.Common.Identity;

public interface IUserService
{
    string GetUserIdentity();
    Task<IUser> GetUserInfo(CancellationToken cancellationToken = default);
}