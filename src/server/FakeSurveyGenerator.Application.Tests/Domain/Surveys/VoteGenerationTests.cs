using AutoFixture;
using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Domain.Users;
using FakeSurveyGenerator.Application.Features.Surveys;

namespace FakeSurveyGenerator.Application.Tests.Domain.Surveys;

public sealed class VoteGenerationTests
{
    [Test]
    public async Task GivenMaximumRespondents_WhenCalculatingRandomOutcomeTwice_ThenEveryVoteIsAssignedOnce()
    {
        var survey = CreateSurvey();
        survey.AddSurveyOption(NonEmptyString.Create("Tabs"));
        survey.AddSurveyOption(NonEmptyString.Create("Spaces"));

        survey.CalculateOutcome();
        await Assert.That(survey.Options.Sum(option => option.NumberOfVotes)).IsEqualTo(survey.NumberOfRespondents);
        survey.CalculateOutcome();
        await Assert.That(survey.Options.Sum(option => option.NumberOfVotes)).IsEqualTo(survey.NumberOfRespondents);
    }

    [Test]
    public async Task GivenPreferredVotes_WhenCalculatingFixedOutcomeTwice_ThenUnassignedVotesRemainUnassigned()
    {
        var survey = CreateSurvey();
        survey.AddSurveyOption(NonEmptyString.Create("Tabs"), 800_000);
        survey.AddSurveyOption(NonEmptyString.Create("Spaces"), 100_000);
        survey.AddSurveyOption(NonEmptyString.Create("Neither"));

        survey.CalculateOutcome();
        survey.CalculateOutcome();

        await Assert.That(survey.Options.Select(option => option.NumberOfVotes)).IsEquivalentTo([800_000, 100_000, 0]);
    }

    [Test]
    public async Task GivenExistingVotes_WhenCalculatingOneSidedOutcome_ThenOnlyOneOptionReceivesAllVotes()
    {
        var survey = CreateSurvey();
        survey.AddSurveyOption(NonEmptyString.Create("Tabs"), 800_000);
        survey.AddSurveyOption(NonEmptyString.Create("Spaces"), 100_000);
        survey.CalculateOutcome();

        survey.CalculateOneSidedOutcome();
        survey.CalculateOneSidedOutcome();

        await Assert.That(survey.Options.Count(option => option.NumberOfVotes > 0)).IsEqualTo(1);
        await Assert.That(survey.Options.Sum(option => option.NumberOfVotes)).IsEqualTo(survey.NumberOfRespondents);
    }

    private static Survey CreateSurvey() => new(new Fixture().Create<User>(), NonEmptyString.Create("Tabs or spaces?"),
        CreateSurveyCommandValidator.MaximumRespondents, NonEmptyString.Create("Developers"));
}
