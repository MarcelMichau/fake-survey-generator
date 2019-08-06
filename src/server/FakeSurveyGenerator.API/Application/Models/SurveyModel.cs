using System;
using System.Collections.Generic;

namespace FakeSurveyGenerator.API.Application.Models
{
    public class SurveyModel
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public string RespondentType { get; set; }
        public int NumberOfRespondents { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<SurveyOptionModel> Options { get; set; }
    }
}
