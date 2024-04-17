using FakeSurveyGenerator.Application.Domain.Surveys;

namespace FakeSurveyGenerator.Application.Features.Surveys;

public sealed record SurveyOptionModel
{
    public required string OptionText { get; init; }
    public required int NumberOfVotes { get; init; }
    public required int PreferredNumberOfVotes { get; init; }
}

public static class SurveyOptionModelMappingExtensions
{
    public static SurveyOptionModel MapToModel(this SurveyOption surveyOption)
    {
        return new SurveyOptionModel
        {
            OptionText = surveyOption.OptionText.Value,
            NumberOfVotes = surveyOption.NumberOfVotes,
            PreferredNumberOfVotes = surveyOption.PreferredNumberOfVotes
        };
    }
}