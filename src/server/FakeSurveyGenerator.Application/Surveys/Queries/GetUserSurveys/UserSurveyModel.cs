using System.Linq;
using AutoMapper;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Application.Surveys.Queries.GetUserSurveys
{
    public sealed class UserSurveyModel : IMapFrom<Survey>
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public string RespondentType { get; set; }
        public int NumberOfRespondents { get; set; }
        public int NumberOfOptions { get; set; }
        public string WinningOption { get; set; }
        public int WinningOptionNumberOfVotes { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Survey, UserSurveyModel>()
                .ForMember(dest => dest.Topic, opts => opts.MapFrom(src => src.Topic.Value))
                .ForMember(dest => dest.RespondentType, opts => opts.MapFrom(src => src.RespondentType.Value))
                .ForMember(dest => dest.NumberOfOptions, opts => opts.MapFrom(src => src.Options.ToList().Count))
                .ForMember(dest => dest.WinningOption, opts => opts.MapFrom(src => src.Options.ToList().OrderByDescending(option => option.NumberOfVotes).First().OptionText.Value))
                .ForMember(dest => dest.WinningOptionNumberOfVotes, opts => opts.MapFrom(src => src.Options.Max(option => option.NumberOfVotes)));
        }
    }
}
