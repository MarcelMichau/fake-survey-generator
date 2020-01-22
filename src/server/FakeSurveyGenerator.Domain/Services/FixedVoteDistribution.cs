using System;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Domain.Services
{
    public class FixedVoteDistribution : IVoteDistribution
    {
        public void DistributeVotes(Survey survey)
        {
            if (survey == null)
                throw new ArgumentException(nameof(survey));

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
