using FakeSurveyGenerator.Application.Common.Identity;

namespace FakeSurveyGenerator.Infrastructure.Identity;

internal sealed class UnauthorizedUser : IUser
{
    public string Id => "unauthorized-user";
    public string DisplayName => "Unauthorized User";
    public string EmailAddress => "";
}