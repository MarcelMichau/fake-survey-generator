using FakeSurveyGenerator.Application.Features.Surveys;

namespace FakeSurveyGenerator.Application.Tests.Features.Surveys;

public sealed class SurveyWorkloadLimitTests
{
    [Test]
    [Arguments(1_000_000, 100, true)]
    [Arguments(1_000_001, 100, false)]
    [Arguments(1_000_000, 101, false)]
    public async Task GivenWorkloadLimits_WhenValidating_ThenOnlyBoundedWorkIsAccepted(int respondents, int options, bool expected)
    {
        var command = new CreateSurveyCommand
        {
            SurveyTopic = "Pick an option",
            RespondentType = "Developers",
            NumberOfRespondents = respondents,
            SurveyOptions = Enumerable.Range(0, options).Select(index => new SurveyOptionDto { OptionText = $"Option {index}" }).ToArray()
        };

        var result = await new CreateSurveyCommandValidator().ValidateAsync(command);

        await Assert.That(result.IsValid).IsEqualTo(expected);
    }
}
