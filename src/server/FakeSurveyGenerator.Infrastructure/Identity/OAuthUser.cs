using FakeSurveyGenerator.Application.Common.Identity;

namespace FakeSurveyGenerator.Infrastructure.Identity;

internal sealed class OAuthUser : IUser
{
    public string Id { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
    public string EmailAddress { get; init; } = null!;

    public OAuthUser() { } // Used for System.Text.Json deserialization

    public OAuthUser(string id, string displayName, string emailAddress) : this()
    {
        Id = id;
        DisplayName = displayName;
        EmailAddress = emailAddress;
    }
}