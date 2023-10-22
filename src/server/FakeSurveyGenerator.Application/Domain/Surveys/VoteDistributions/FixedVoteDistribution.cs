namespace FakeSurveyGenerator.Application.Domain.Surveys.VoteDistributions;

internal sealed class FixedVoteDistribution : IVoteDistribution
{
    public void DistributeVotes(Survey survey)
    {
        if (survey is null)
            throw new ArgumentNullException(nameof(survey));

        foreach (var surveyOption in survey.Options)
        {
            for (var i = 0; i < surveyOption.PreferredNumberOfVotes; i++)
            {
                surveyOption.AddVote();
            }
        }
    }
}