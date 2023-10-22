using FakeSurveyGenerator.Application.Shared.Identity;

namespace FakeSurveyGenerator.Application.Infrastructure.Identity;

internal sealed class UnidentifiedUser : IUser
{
    public string Id => "unidentified-user";
    public string DisplayName => "Unidentified User";
    public string EmailAddress => "";
}