using System;
using System.Linq;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Exceptions;
using FakeSurveyGenerator.Domain.Services;
using Xunit;

namespace FakeSurveyGenerator.Domain.Tests
{
    public class SurveyTests
    {
        [Fact]
        public void Should_Be_Able_To_Create_Survey()
        {
            var topic = "Tabs or spaces?";
            var numberOfRespondents = 1;
            var respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            Assert.Equal(topic, survey.Topic);
            Assert.Equal(numberOfRespondents, survey.NumberOfRespondents);
            Assert.Equal(respondentType, survey.RespondentType);
            Assert.True(survey.CreatedOn < DateTime.UtcNow, "The createdOn date was not in the past");
        }

        [Fact]
        public void Should_Not_Be_Able_To_Create_Survey_With_No_Topic()
        {
            var topic = "";
            var numberOfRespondents = 1;
            var respondentType = "Developers";

            Assert.Throws<SurveyDomainException>(() => new Survey(topic, numberOfRespondents, respondentType));
        }

        [Fact]
        public void Should_Not_Be_Able_To_Create_Survey_With_No_Respondents()
        {
            var topic = "To be, or not to be?";
            var numberOfRespondents = 0;
            var respondentType = "Writers";

            Assert.Throws<SurveyDomainException>(() => new Survey(topic, numberOfRespondents, respondentType));
        }

        [Fact]
        public void Should_Not_Be_Able_To_Create_Survey_With_No_Respondent_Type()
        {
            var topic = "To be, or not to be?";
            var numberOfRespondents = 1;
            var respondentType = "";

            Assert.Throws<SurveyDomainException>(() => new Survey(topic, numberOfRespondents, respondentType));
        }

        [Fact]
        public void Should_Be_Able_To_Add_Options_To_Survey()
        {
            var topic = "Tabs or spaces?";
            var numberOfRespondents = 1;
            var respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption("Tabs");
            survey.AddSurveyOption("Spaces");

            Assert.Equal(2, survey.Options.Count);
            Assert.Equal("Tabs", survey.Options.First().OptionText);
            Assert.Equal("Spaces", survey.Options.Last().OptionText);
        }

        [Fact]
        public void Should_Not_Be_Able_To_Add_Empty_Options_To_Survey()
        {
            var topic = "To be, or not to be?";
            var numberOfRespondents = 2;
            var respondentType = "Writers";

            Assert.Throws<SurveyDomainException>(() =>
            {
                var survey = new Survey(topic, numberOfRespondents, respondentType);

                survey.AddSurveyOption("");
            });
        }

        [Fact]
        public void Should_Not_Be_Able_To_Calculate_Results_Of_Survey_With_No_Options()
        {
            var topic = "Tabs or spaces?";
            var numberOfRespondents = 1000;
            var respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            var voteDistributionStrategy = new RandomVoteDistribution();

            Assert.Throws<SurveyDomainException>(() => survey.CalculateOutcome(voteDistributionStrategy));
        }

        [Fact]
        public void Should_Be_Able_To_Calculate_Results_Of_Survey_With_Random_Outcome()
        {
            var topic = "Tabs or spaces?";
            var numberOfRespondents = 1000;
            var respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption("Tabs");
            survey.AddSurveyOption("Spaces");

            var voteDistributionStrategy = new RandomVoteDistribution();

            var result = survey.CalculateOutcome(voteDistributionStrategy);

            Assert.Equal(numberOfRespondents, result.Options.Sum(option => option.NumberOfVotes));
            Assert.True(result.Options.All(option => option.NumberOfVotes > 0));
        }

        [Fact]
        public void Should_Be_Able_To_Calculate_Results_Of_Survey_With_One_Sided_Outcome()
        {
            var topic = "Tabs or spaces?";
            var numberOfRespondents = 1000;
            var respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption("Tabs");
            survey.AddSurveyOption("Spaces");

            var voteDistributionStrategy = new OneSidedVoteDistribution();

            var result = survey.CalculateOutcome(voteDistributionStrategy);

            Assert.Equal(numberOfRespondents, result.Options.Max(option => option.NumberOfVotes));
        }

        [Fact]
        public void Should_Be_Able_To_Calculate_Results_Of_Survey_With_Fixed_Outcome()
        {
            var topic = "Tabs or spaces?";
            var numberOfRespondents = 1000;
            var respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption("Tabs", 600);
            survey.AddSurveyOption("Spaces", 400);

            var voteDistribution = new FixedVoteDistribution();

            var result = survey.CalculateOutcome(voteDistribution);

            Assert.Equal(600, result.Options.First().NumberOfVotes);
            Assert.Equal(400, result.Options.Last().NumberOfVotes);
        }

        [Fact]
        public void Should_Not_Be_Able_To_Add_Preferred_Number_Of_Votes_Greater_Than_Respondents()
        {
            var topic = "Tabs or spaces?";
            var numberOfRespondents = 1000;
            var respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption("Tabs", 1);

            Assert.Throws<SurveyDomainException>(() =>
            {
                survey.AddSurveyOption("Spaces", 1001);
            });
        }

        [Fact]
        public void Should_Not_Be_Able_To_Add_Preferred_Number_Of_Votes_If_Total_Exceeds_Respondents()
        {
            var topic = "Tabs or spaces?";
            var numberOfRespondents = 1000;
            var respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption("Tabs", 500);

            Assert.Throws<SurveyDomainException>(() =>
            {
                survey.AddSurveyOption("Spaces", 501);
            });
        }
    }
}
