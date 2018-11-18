using AutoMapper;
using FakeSurveyGenerator.API.Models;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.API
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Survey, SurveyModel>();
            CreateMap<SurveyOption, SurveyOptionModel>();
        }
    }
}
