using FakeSurveyGenerator.Application.Common.Identity;

namespace FakeSurveyGenerator.Infrastructure.Identity
{
    public sealed class UnidentifiedUser : IUser
    {
        public string Id => "unidentified-user";
        public string DisplayName => "Unidentified User";
        public string EmailAddress => "";
    }
}
