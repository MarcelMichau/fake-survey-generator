namespace FakeSurveyGenerator.Application.Domain.Surveys.VoteDistributions;

internal sealed class FixedVoteDistribution : IVoteDistribution
{
    public void DistributeVotes(Survey survey)
    {
        ArgumentNullException.ThrowIfNull(survey);

        foreach (var surveyOption in survey.Options)
            for (var i = 0; i < surveyOption.PreferredNumberOfVotes; i++)
                surveyOption.AddVote();
    }
}