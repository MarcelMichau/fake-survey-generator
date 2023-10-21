using AutoMapper;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Shared.Mappings;

namespace FakeSurveyGenerator.Application.Features.Surveys;

public sealed record SurveyOptionModel : IMapFrom<SurveyOption>
{
    public string OptionText { get; init; } = null!;
    public int NumberOfVotes { get; init; }
    public int PreferredNumberOfVotes { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<SurveyOption, SurveyOptionModel>()
            .ForMember(dest => dest.OptionText, opts => opts.MapFrom(src => src.OptionText.Value));
    }
}