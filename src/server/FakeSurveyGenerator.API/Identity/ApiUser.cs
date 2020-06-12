using FakeSurveyGenerator.Application.Common.Identity;

namespace FakeSurveyGenerator.API.Identity
{
    public class ApiUser : IUser
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string EmailAddress { get; }

        public ApiUser(string id, string displayName, string emailAddress)
        {
            Id = id;
            DisplayName = displayName;
            EmailAddress = emailAddress;
        }
    }
}
