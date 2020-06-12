using AutoMapper;
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

        public void Mapping(Profile profile)
        {
            profile.CreateMap<User, UserModel>()
                .ForMember(dest => dest.DisplayName, opts => opts.MapFrom(src => src.DisplayName.Value))
                .ForMember(dest => dest.EmailAddress, opts => opts.MapFrom(src => src.EmailAddress.Value))
                .ForMember(dest => dest.ExternalUserId, opts => opts.MapFrom(src => src.ExternalUserId.Value));
        }
    }
}