namespace FakeSurveyGenerator.Application.Domain.Surveys.VoteDistributions;

internal sealed class RandomVoteDistribution : IVoteDistribution
{
    public void DistributeVotes(Survey survey)
    {
        ArgumentNullException.ThrowIfNull(survey);

        for (var i = 0; i < survey.NumberOfRespondents; i++)
        {
            var randomIndex = Random.Shared.Next(0, survey.Options.Count);
            survey.Options[randomIndex].AddVote();
        }
    }
}