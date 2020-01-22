using System;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Domain.Services
{
    public class OneSidedVoteDistribution : IVoteDistribution
    {
        public void DistributeVotes(Survey survey)
        {
            if (survey == null)
                throw new ArgumentException(nameof(survey));

            var random = new Random();

            var winningOptionIndex = random.Next(0, survey.Options.Count);

            for (var i = 0; i < survey.NumberOfRespondents; i++)
            {
                survey.Options[winningOptionIndex].AddVote();
            }
        }
    }
}
