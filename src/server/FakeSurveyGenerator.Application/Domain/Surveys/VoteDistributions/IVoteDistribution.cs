namespace FakeSurveyGenerator.Application.Domain.Surveys.VoteDistributions;

internal interface IVoteDistribution
{
    void DistributeVotes(Survey survey);
}