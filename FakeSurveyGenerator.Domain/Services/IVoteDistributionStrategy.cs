using System.Collections.Generic;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Domain.Services
{
    public interface IVoteDistributionStrategy
    {
        void DistributeVotes(List<SurveyOption> surveyOptions, int numberOfRespondents);
    }
}
