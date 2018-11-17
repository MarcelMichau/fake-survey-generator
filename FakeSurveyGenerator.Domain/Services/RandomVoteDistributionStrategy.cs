using System;
using System.Collections.Generic;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Domain.Services
{
    public class RandomVoteDistributionStrategy : IVoteDistributionStrategy
    {
        public void DistributeVotes(List<SurveyOption> surveyOptions, int numberOfRespondents)
        {
            var random = new Random();

            for (var i = 0; i < numberOfRespondents; i++)
            {
                var randomIndex = random.Next(0, surveyOptions.Count);
                surveyOptions[randomIndex].AddVote();
            }
        }
    }
}
