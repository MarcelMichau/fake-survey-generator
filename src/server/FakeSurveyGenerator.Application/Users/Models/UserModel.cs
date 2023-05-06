using AutoMapper;
using FakeSurveyGenerator.Application.Common.Auditing;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;

namespace FakeSurveyGenerator.Application.Users.Models;

public sealed record UserModel : AuditableModel, IMapFrom<User>
{
    public int Id { get; init; }
    public string DisplayName { get; init; } = null!;
    public string EmailAddress { get; init; } = null!;
    public string ExternalUserId { get; init; } = null!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserModel>()
            .ForMember(dest => dest.DisplayName, opts => opts.MapFrom(src => src.DisplayName.Value))
            .ForMember(dest => dest.EmailAddress, opts => opts.MapFrom(src => src.EmailAddress.Value))
            .ForMember(dest => dest.ExternalUserId, opts => opts.MapFrom(src => src.ExternalUserId.Value));
    }
}