using System;
using System.Collections.Generic;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Domain.Services
{
    public class OneSidedVoteDistributionStrategy : IVoteDistributionStrategy
    {
        public void DistributeVotes(List<SurveyOption> surveyOptions, int numberOfRespondents)
        {
            var random = new Random();

            var winningOptionIndex = random.Next(0, surveyOptions.Count);

            for (var i = 0; i < numberOfRespondents; i++)
            {
                surveyOptions[winningOptionIndex].AddVote();
            }
        }
    }
}
