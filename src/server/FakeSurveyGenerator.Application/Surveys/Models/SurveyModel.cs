using System.Collections.Generic;
using AutoMapper;
using FakeSurveyGenerator.Application.Common.Auditing;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Application.Surveys.Models
{
    public sealed class SurveyModel : AuditableModel, IMapFrom<Survey>
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string Topic { get; set; }
        public string RespondentType { get; set; }
        public int NumberOfRespondents { get; set; }
        public List<SurveyOptionModel> Options { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Survey, SurveyModel>()
                .ForMember(dest => dest.OwnerId, opts => opts.MapFrom(src => src.Owner.Id))
                .ForMember(dest => dest.Topic, opts => opts.MapFrom(src => src.Topic.Value))
                .ForMember(dest => dest.RespondentType, opts => opts.MapFrom(src => src.RespondentType.Value));
        }
    }
}