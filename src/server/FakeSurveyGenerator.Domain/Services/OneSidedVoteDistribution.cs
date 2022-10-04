using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Domain.Services;

internal sealed class OneSidedVoteDistribution : IVoteDistribution
{
    public void DistributeVotes(Survey survey)
    {
        if (survey is null)
            throw new ArgumentNullException(nameof(survey));

        var winningOptionIndex = Random.Shared.Next(0, survey.Options.Count);

        for (var i = 0; i < survey.NumberOfRespondents; i++)
        {
            survey.Options[winningOptionIndex].AddVote();
        }
    }
}