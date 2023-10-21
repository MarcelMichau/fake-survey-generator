using FakeSurveyGenerator.Application.Shared.Identity;

namespace FakeSurveyGenerator.Application.Infrastructure.Identity;

internal sealed class SystemUser : IUser
{
    public string Id => "system-user";
    public string DisplayName => "System User";
    public string EmailAddress => "system.user@test.com";
}