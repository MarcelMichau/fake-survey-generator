using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Surveys.Models;
using MediatR;

namespace FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey
{
    // Ideally, commands should be immutable & not have public setters on properties, but System.Text.Json
    // cannot set these properties when serializing the command, so a compromise had to be made. :(
    public sealed class CreateSurveyCommand : IRequest<Result<SurveyModel, Error>>
    {
        public string SurveyTopic { get; set; }

        public int NumberOfRespondents { get; set; }

        public string RespondentType { get; set; }

        public IEnumerable<SurveyOptionDto> SurveyOptions { get; set; }

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

    public sealed class SurveyOptionDto
    {
        public string OptionText { get; set; }
        public int PreferredNumberOfVotes { get; set; }
    }
}
