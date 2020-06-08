using System;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Domain.Services
{
    internal sealed class RandomVoteDistribution : IVoteDistribution
    {
        public void DistributeVotes(Survey survey)
        {
            if (survey is null)
                throw new ArgumentNullException(nameof(survey));

            var random = new Random();

            for (var i = 0; i < survey.NumberOfRespondents; i++)
            {
                var randomIndex = random.Next(0, survey.Options.Count);
                survey.Options[randomIndex].AddVote();
            }
        }
    }
}
