using AutoMapper;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Application.Surveys.Models
{
    public sealed class SurveyOptionModel : IMapFrom<SurveyOption>
    {
        public int Id { get; set; }
        public string OptionText { get; set; }
        public int NumberOfVotes { get; set; }
        public int PreferredNumberOfVotes { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<SurveyOption, SurveyOptionModel>()
                .ForMember(dest => dest.OptionText, opts => opts.MapFrom(src => src.OptionText.Value));
        }
    }
}
