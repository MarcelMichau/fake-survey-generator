using System;
using System.Collections.Generic;
using AutoMapper;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Application.Surveys.Models
{
    public sealed class SurveyModel : IMapFrom<Survey>
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public string RespondentType { get; set; }
        public int NumberOfRespondents { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<SurveyOptionModel> Options { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Survey, SurveyModel>()
                .ForMember(dest => dest.Topic, opts => opts.MapFrom(src => src.Topic.Value))
                .ForMember(dest => dest.RespondentType, opts => opts.MapFrom(src => src.RespondentType.Value));
        }
    }
}