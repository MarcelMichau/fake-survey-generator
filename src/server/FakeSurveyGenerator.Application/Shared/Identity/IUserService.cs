namespace FakeSurveyGenerator.Application.Shared.Identity;

public interface IUserService
{
    string GetUserIdentity();
    Task<IUser> GetUserInfo(CancellationToken cancellationToken = default);
}