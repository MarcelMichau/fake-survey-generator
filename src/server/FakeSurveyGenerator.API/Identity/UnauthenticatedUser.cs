using FakeSurveyGenerator.Application.Common.Identity;

namespace FakeSurveyGenerator.API.Identity
{
    public class UnauthenticatedUser : IUser
    {
        public string Id => "Unauthenticated";
        public string DisplayName => "Unauthenticated";
        public string EmailAddress => "Unauthenticated";
    }
}
