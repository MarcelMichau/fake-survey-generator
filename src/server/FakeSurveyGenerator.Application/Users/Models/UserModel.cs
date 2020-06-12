using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;

namespace FakeSurveyGenerator.Application.Users.Models
{
    public sealed class UserModel : IMapFrom<User>
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public string ExternalUserId { get; set; }
    }
}