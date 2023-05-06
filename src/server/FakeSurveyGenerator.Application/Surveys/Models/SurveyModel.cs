using AutoMapper;
using FakeSurveyGenerator.Application.Common.Auditing;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Application.Surveys.Models;

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