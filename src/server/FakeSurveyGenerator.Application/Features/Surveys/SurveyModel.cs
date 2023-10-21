using AutoMapper;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Shared.Auditing;
using FakeSurveyGenerator.Application.Shared.Mappings;

namespace FakeSurveyGenerator.Application.Features.Surveys;

public sealed record SurveyModel : AuditableModel, IMapFrom<Survey>
{
    public int Id { get; init; }
    public int OwnerId { get; init; }
    public string Topic { get; init; } = null!;
    public string RespondentType { get; init; } = null!;
    public int NumberOfRespondents { get; init; }
    public List<SurveyOptionModel> Options { get; init; } = null!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Survey, SurveyModel>()
            .ForMember(dest => dest.OwnerId, opts => opts.MapFrom(src => src.Owner.Id))
            .ForMember(dest => dest.Topic, opts => opts.MapFrom(src => src.Topic.Value))
            .ForMember(dest => dest.RespondentType, opts => opts.MapFrom(src => src.RespondentType.Value));
    }
}