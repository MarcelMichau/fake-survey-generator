using System;
using System.Linq;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Events;
using FakeSurveyGenerator.Domain.Exceptions;
using Xunit;

namespace FakeSurveyGenerator.Domain.Tests
{
    public class SurveyTests
    {
        [Fact]
        public void Should_Be_Able_To_Create_Survey()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1;
            const string respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            Assert.Equal(topic, survey.Topic);
            Assert.Equal(numberOfRespondents, survey.NumberOfRespondents);
            Assert.Equal(respondentType, survey.RespondentType);
            Assert.True(survey.CreatedOn < DateTime.UtcNow, "The createdOn date was not in the past");
        }

        [Fact]
        public void Should_Not_Be_Able_To_Create_Survey_With_No_Topic()
        {
            const string topic = "";
            const int numberOfRespondents = 1;
            const string respondentType = "Developers";

            Assert.Throws<SurveyDomainException>(() => new Survey(topic, numberOfRespondents, respondentType));
        }

        [Fact]
        public void Should_Not_Be_Able_To_Create_Survey_With_No_Respondents()
        {
            const string topic = "To be, or not to be?";
            const int numberOfRespondents = 0;
            const string respondentType = "Writers";

            Assert.Throws<SurveyDomainException>(() => new Survey(topic, numberOfRespondents, respondentType));
        }

        [Fact]
        public void Should_Not_Be_Able_To_Create_Survey_With_No_Respondent_Type()
        {
            const string topic = "To be, or not to be?";
            const int numberOfRespondents = 1;
            const string respondentType = "";

            Assert.Throws<SurveyDomainException>(() => new Survey(topic, numberOfRespondents, respondentType));
        }

        [Fact]
        public void Should_Be_Able_To_Add_Options_To_Survey()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1;
            const string respondentType = "Developers";

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
            const string topic = "To be, or not to be?";
            const int numberOfRespondents = 2;
            const string respondentType = "Writers";

            Assert.Throws<SurveyDomainException>(() =>
            {
                var survey = new Survey(topic, numberOfRespondents, respondentType);

                survey.AddSurveyOption("");
            });
        }

        [Fact]
        public void Should_Not_Be_Able_To_Calculate_Results_Of_Survey_With_No_Options()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            Assert.Throws<SurveyDomainException>(() => survey.CalculateOutcome());
        }

        [Fact]
        public void Should_Be_Able_To_Calculate_Results_Of_Survey_With_Random_Outcome()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption("Tabs");
            survey.AddSurveyOption("Spaces");

            var result = survey.CalculateOutcome();

            Assert.Equal(numberOfRespondents, result.Options.Sum(option => option.NumberOfVotes));
            Assert.True(result.Options.All(option => option.NumberOfVotes > 0));
        }

        [Fact]
        public void Should_Be_Able_To_Calculate_Results_Of_Survey_With_One_Sided_Outcome()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption("Tabs");
            survey.AddSurveyOption("Spaces");

            var result = survey.CalculateOneSidedOutcome();

            Assert.Equal(numberOfRespondents, result.Options.Max(option => option.NumberOfVotes));
        }

        [Fact]
        public void Should_Be_Able_To_Calculate_Results_Of_Survey_With_Fixed_Outcome()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption("Tabs", 600);
            survey.AddSurveyOption("Spaces", 400);

            var result = survey.CalculateOutcome();

            Assert.Equal(600, result.Options.First().NumberOfVotes);
            Assert.Equal(400, result.Options.Last().NumberOfVotes);
        }

        [Fact]
        public void Should_Not_Be_Able_To_Add_Preferred_Number_Of_Votes_Greater_Than_Respondents()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

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
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption("Tabs", 500);

            Assert.Throws<SurveyDomainException>(() =>
            {
                survey.AddSurveyOption("Spaces", 501);
            });
        }

        [Fact]
        public void Creating_Survey_Should_Add_SurveyCreated_Event_To_DomainEvents()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1;
            const string respondentType = "Developers";

            var survey = new Survey(topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption("Tabs");
            survey.AddSurveyOption("Spaces");


            var surveyCreatedEvent = survey.DomainEvents.First() as SurveyCreatedDomainEvent;

            Assert.True(surveyCreatedEvent?.Survey == survey);
        }
    }
}
