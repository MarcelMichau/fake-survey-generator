namespace FakeSurveyGenerator.Application.Domain.Surveys.VoteDistributions;

internal sealed class OneSidedVoteDistribution : IVoteDistribution
{
    public void DistributeVotes(Survey survey)
    {
        ArgumentNullException.ThrowIfNull(survey);

        var winningOptionIndex = Random.Shared.Next(0, survey.Options.Count);

        for (var i = 0; i < survey.NumberOfRespondents; i++)
        {
            survey.Options[winningOptionIndex].AddVote();
        }
    }
}