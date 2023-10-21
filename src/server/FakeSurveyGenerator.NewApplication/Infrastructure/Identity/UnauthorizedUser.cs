using FakeSurveyGenerator.Application.Shared.Identity;

namespace FakeSurveyGenerator.Application.Infrastructure.Identity;

internal sealed class UnauthorizedUser : IUser
{
    public string Id => "unauthorized-user";
    public string DisplayName => "Unauthorized User";
    public string EmailAddress => "";
}