using FakeSurveyGenerator.Application.Common.Identity;

namespace FakeSurveyGenerator.Infrastructure.Identity
{
    internal class OAuthUser : IUser
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string EmailAddress { get; }

        public OAuthUser(string id, string displayName, string emailAddress)
        {
            Id = id;
            DisplayName = displayName;
            EmailAddress = emailAddress;
        }
    }
}
