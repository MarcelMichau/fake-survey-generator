using System;
using System.Collections.Generic;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Application.Surveys.Models
{
    public class SurveyModel : IMapFrom<Survey>
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public string RespondentType { get; set; }
        public int NumberOfRespondents { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<SurveyOptionModel> Options { get; set; }

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }
}
