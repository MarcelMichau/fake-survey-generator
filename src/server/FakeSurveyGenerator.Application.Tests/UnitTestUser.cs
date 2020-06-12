using FakeSurveyGenerator.Application.Common.Identity;

namespace FakeSurveyGenerator.Application.Tests
{
    internal sealed class UnitTestUser : IUser
    {
        public string Id => "test-id";
        public string DisplayName => "Unit Test User";
        public string EmailAddress => "test.user@test.com";
    }
}
