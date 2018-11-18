using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Domain.Services
{
    public interface IVoteDistributionStrategy
    {
        void DistributeVotes(Survey survey);
    }
}
