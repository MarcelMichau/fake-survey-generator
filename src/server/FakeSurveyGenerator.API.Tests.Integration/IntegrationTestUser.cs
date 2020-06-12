using FakeSurveyGenerator.Application.Common.Identity;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    public class IntegrationTestUser : IUser
    {
        public string Id => "test-id";
        public string DisplayName => "Test User";
        public string EmailAddress => "test.user@test.com";
    }
}
