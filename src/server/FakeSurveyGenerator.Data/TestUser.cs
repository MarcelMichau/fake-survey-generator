using FakeSurveyGenerator.Application.Common.Identity;

namespace FakeSurveyGenerator.Data
{
    public sealed class TestUser : IUser
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string EmailAddress { get; }

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
