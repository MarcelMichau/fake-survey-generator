using FakeSurveyGenerator.Application.Common.Identity;

namespace FakeSurveyGenerator.Infrastructure.Identity;

internal sealed class OAuthUser : IUser
{
    public string Id { get; init; }
    public string DisplayName { get; init; }
    public string EmailAddress { get; init; }

    public OAuthUser() { } // Used for System.Text.Json deserialization

    public OAuthUser(string id, string displayName, string emailAddress) : this()
    {
        Id = id;
        DisplayName = displayName;
        EmailAddress = emailAddress;
    }
}