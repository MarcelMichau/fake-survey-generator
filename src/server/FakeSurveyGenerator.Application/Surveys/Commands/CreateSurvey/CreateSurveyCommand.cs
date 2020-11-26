using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Surveys.Models;
using MediatR;

namespace FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey
{
    public sealed class CreateSurveyCommand : IRequest<Result<SurveyModel, Error>>
    {
        public string SurveyTopic { get; init; }

        public int NumberOfRespondents { get; init; }

        public string RespondentType { get; init; }

        public IEnumerable<SurveyOptionDto> SurveyOptions { get; init; }

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
        public string OptionText { get; init; }
        public int PreferredNumberOfVotes { get; init; }
    }
}
