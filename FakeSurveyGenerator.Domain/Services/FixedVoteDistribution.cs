using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Domain.Services
{
    public class FixedVoteDistribution : IVoteDistribution
    {
        public void DistributeVotes(Survey survey)
        {
            foreach (var surveyOption in survey.Options)
            {
                for (var i = 0; i < surveyOption.PreferredNumberOfVotes; i++)
                {
                    surveyOption.AddVote();
                }
            }
        }
    }
}
