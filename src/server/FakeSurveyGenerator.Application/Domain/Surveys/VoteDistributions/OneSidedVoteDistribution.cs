namespace FakeSurveyGenerator.Application.Domain.Surveys.VoteDistributions;

internal sealed class OneSidedVoteDistribution : IVoteDistribution
{
    public void DistributeVotes(Survey survey)
    {
        ArgumentNullException.ThrowIfNull(survey);

        var winningOptionIndex = Random.Shared.Next(0, survey.Options.Count);

        survey.Options[winningOptionIndex].AddVotes(survey.NumberOfRespondents);
    }
}
