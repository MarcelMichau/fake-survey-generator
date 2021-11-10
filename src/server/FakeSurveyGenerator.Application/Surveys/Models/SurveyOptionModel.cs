using AutoMapper;
using FakeSurveyGenerator.Application.Common.Auditing;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Application.Surveys.Models;

public sealed record SurveyOptionModel : AuditableModel, IMapFrom<SurveyOption>
{
    public int Id { get; init; }
    public string OptionText { get; init; }
    public int NumberOfVotes { get; init; }
    public int PreferredNumberOfVotes { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<SurveyOption, SurveyOptionModel>()
            .ForMember(dest => dest.OptionText, opts => opts.MapFrom(src => src.OptionText.Value));
    }
}