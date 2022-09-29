using System;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Domain.Services;

internal sealed class RandomVoteDistribution : IVoteDistribution
{
    public void DistributeVotes(Survey survey)
    {
        if (survey is null)
            throw new ArgumentNullException(nameof(survey));

        for (var i = 0; i < survey.NumberOfRespondents; i++)
        {
            var randomIndex = Random.Shared.Next(0, survey.Options.Count);
            survey.Options[randomIndex].AddVote();
        }
    }
}