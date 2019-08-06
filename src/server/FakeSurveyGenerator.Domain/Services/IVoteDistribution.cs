using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Domain.Services
{
    public interface IVoteDistribution
    {
        void DistributeVotes(Survey survey);
    }
}
