using FakeSurveyGenerator.Application.Domain.Surveys;

namespace FakeSurveyGenerator.Application.Features.Surveys;

public sealed record SurveyOptionModel
{
    public string OptionText { get; init; } = null!;
    public int NumberOfVotes { get; init; }
    public int PreferredNumberOfVotes { get; init; }
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