using System.Collections.Generic;

namespace FakeSurveyGenerator.API.Application
{
    public class CreateSurveyCommand
    {
        public string SurveyTopic { get; }
        public int NumberOfRespondents { get; }
        public string RespondentType { get; }
        private readonly List<SurveyOptionDto> _surveyOptions;

        public IEnumerable<SurveyOptionDto> SurveyOptions => _surveyOptions;

        public CreateSurveyCommand(string surveyTopic, int numberOfRespondents, string respondentType, List<SurveyOptionDto> surveyOptions)
        {
            SurveyTopic = surveyTopic;
            NumberOfRespondents = numberOfRespondents;
            RespondentType = respondentType;
            _surveyOptions = surveyOptions;
        }
    }

    public class SurveyOptionDto
    {
        public string OptionText { get; set; }
        public int? PreferredOutcomeRank { get; set; }
    }
}
