using FakeSurveyGenerator.Application.Common.Identity;

namespace FakeSurveyGenerator.Data
{
    public sealed record TestUser : IUser
    {
        public string Id { get; init; }
        public string DisplayName { get; init; }
        public string EmailAddress { get; init; }

        public TestUser()
        {
            Id = "test-id";
            DisplayName = "Test User";
            EmailAddress = "test.user@test.com";
        }

        public TestUser(string id, string displayName, string emailAddress)
        {
            Id = id;
            DisplayName = displayName;
            EmailAddress = emailAddress;
        }
    }
}
