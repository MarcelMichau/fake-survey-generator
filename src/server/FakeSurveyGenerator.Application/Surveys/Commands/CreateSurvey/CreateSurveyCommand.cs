using System.Collections.Generic;
using System.Runtime.Serialization;
using FakeSurveyGenerator.Application.Surveys.Models;
using MediatR;

namespace FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey
{
    [DataContract]
    public class CreateSurveyCommand : IRequest<SurveyModel>
    {
        [DataMember]
        public string SurveyTopic { get; private set; }

        [DataMember]
        public int NumberOfRespondents { get; private set; }

        [DataMember]
        public string RespondentType { get; private set; }

        [DataMember]
        public IEnumerable<SurveyOptionDto> SurveyOptions { get; private set; }

        public CreateSurveyCommand()
        {
             SurveyOptions = new List<SurveyOptionDto>();
        }

        public CreateSurveyCommand(string surveyTopic, int numberOfRespondents, string respondentType, IEnumerable<SurveyOptionDto> surveyOptions) : this()
        {
            SurveyTopic = surveyTopic;
            NumberOfRespondents = numberOfRespondents;
            RespondentType = respondentType;
            SurveyOptions = surveyOptions;
        }
    }

    public class SurveyOptionDto
    {
        public string OptionText { get; set; }
        public int? PreferredNumberOfVotes { get; set; }
    }
}
