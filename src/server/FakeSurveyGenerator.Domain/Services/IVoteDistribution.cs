using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Domain.Services
{
    internal interface IVoteDistribution
    {
        void DistributeVotes(Survey survey);
    }
}
