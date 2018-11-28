using System;
using System.Collections.Generic;
using System.Linq;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Domain.Services
{
    public class RankedVoteDistribution : IVoteDistribution
    {
        //public void DistributeVotes(Survey survey)
        //{
        //    var votes = new List<int>();

        //    var votesSoFar = 0;

        //    var random = new Random();

        //    for (var i = 0; i < survey.Options.Count; i++)
        //    {
        //        if (i == survey.Options.Count - 1)
        //        {
        //            votes.Add(survey.NumberOfRespondents - votesSoFar);
        //            votesSoFar += survey.NumberOfRespondents - votesSoFar;
        //            continue;
        //        }

        //        var randomVotes = random.Next(0, survey.NumberOfRespondents - votesSoFar);

        //        votes.Add(randomVotes);

        //        votesSoFar += randomVotes;
        //    }

        //    votes.Sort((x, y) => y - x);

        //    var index = 0;

        //    foreach (var option in survey.Options.OrderBy(option => option.PreferredOutcomeRank))
        //    {
        //        for (var i = 0; i < votes[index]; i++)
        //        {
        //            option.AddVote();
        //        }

        //        index++;
        //    }
        //}
        public void DistributeVotes(Survey survey)
        {
            foreach (var option in survey.Options)
            {
                var votes = Math.Round((1 - decimal.Divide(option.PreferredOutcomeRank, survey.Options.Count)) * survey.NumberOfRespondents);

                for (var i = 0; i < votes; i++)
                {
                    option.AddVote();
                }
            }
        }
    }
}
