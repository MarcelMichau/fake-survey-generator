using System;
using System.Linq;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Common;
using FakeSurveyGenerator.Domain.DomainEvents;
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

            var survey = new Survey(NonEmptyString.Create(topic), numberOfRespondents, NonEmptyString.Create(respondentType));

            Assert.Equal(topic, survey.Topic);
            Assert.Equal(numberOfRespondents, survey.NumberOfRespondents);
            Assert.Equal(respondentType, survey.RespondentType);
            Assert.True(survey.CreatedOn < DateTime.UtcNow, "The CreatedOn date was not in the past");
        }

        [Fact]
        public void Should_Not_Be_Able_To_Create_Survey_With_No_Topic()
        {
            const string topic = "";
            const int numberOfRespondents = 1;
            const string respondentType = "Developers";

            Assert.ThrowsAny<Exception>(() => new Survey(NonEmptyString.Create(topic), numberOfRespondents, NonEmptyString.Create(respondentType)));
        }

        [Fact]
        public void Should_Not_Be_Able_To_Create_Survey_With_No_Respondents()
        {
            const string topic = "To be, or not to be?";
            const int numberOfRespondents = 0;
            const string respondentType = "Writers";

            Assert.Throws<SurveyDomainException>(() => new Survey(NonEmptyString.Create(topic), numberOfRespondents, NonEmptyString.Create(respondentType)));
        }

        [Fact]
        public void Should_Not_Be_Able_To_Create_Survey_With_No_Respondent_Type()
        {
            const string topic = "To be, or not to be?";
            const int numberOfRespondents = 1;
            const string respondentType = "";

            Assert.ThrowsAny<Exception>(() => new Survey(NonEmptyString.Create(topic), numberOfRespondents, NonEmptyString.Create(respondentType)));
        }

        [Fact]
        public void Should_Be_Able_To_Add_Options_To_Survey()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1;
            const string respondentType = "Developers";

            var survey = new Survey(NonEmptyString.Create(topic), numberOfRespondents, NonEmptyString.Create(respondentType));

            survey.AddSurveyOption(NonEmptyString.Create("Tabs"));
            survey.AddSurveyOption(NonEmptyString.Create("Spaces"));

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

            Assert.ThrowsAny<Exception>(() =>
            {
                var survey = new Survey(NonEmptyString.Create(topic), numberOfRespondents, NonEmptyString.Create(respondentType));

                survey.AddSurveyOption(NonEmptyString.Create(""));
            });
        }

        [Fact]
        public void Should_Not_Be_Able_To_Calculate_Results_Of_Survey_With_No_Options()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(NonEmptyString.Create(topic), numberOfRespondents, NonEmptyString.Create(respondentType));

            Assert.Throws<SurveyDomainException>(() => survey.CalculateOutcome());
        }

        [Fact]
        public void Should_Be_Able_To_Calculate_Results_Of_Survey_With_Random_Outcome()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(NonEmptyString.Create(topic), numberOfRespondents, NonEmptyString.Create(respondentType));

            survey.AddSurveyOption(NonEmptyString.Create("Tabs"));
            survey.AddSurveyOption(NonEmptyString.Create("Spaces"));

            survey.CalculateOutcome();

            Assert.Equal(numberOfRespondents, survey.Options.Sum(option => option.NumberOfVotes));
            Assert.True(survey.Options.All(option => option.NumberOfVotes > 0));
        }

        [Fact]
        public void Should_Be_Able_To_Calculate_Results_Of_Survey_With_One_Sided_Outcome()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(NonEmptyString.Create(topic), numberOfRespondents, NonEmptyString.Create(respondentType));

            survey.AddSurveyOption(NonEmptyString.Create("Tabs"));
            survey.AddSurveyOption(NonEmptyString.Create("Spaces"));

            survey.CalculateOneSidedOutcome();

            Assert.Equal(numberOfRespondents, survey.Options.Max(option => option.NumberOfVotes));
        }

        [Fact]
        public void Should_Be_Able_To_Calculate_Results_Of_Survey_With_Fixed_Outcome()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(NonEmptyString.Create(topic), numberOfRespondents, NonEmptyString.Create(respondentType));

            survey.AddSurveyOption(NonEmptyString.Create("Tabs"), 600);
            survey.AddSurveyOption(NonEmptyString.Create("Spaces"), 400);

            survey.CalculateOutcome();

            Assert.Equal(600, survey.Options.First().NumberOfVotes);
            Assert.Equal(400, survey.Options.Last().NumberOfVotes);
        }

        [Fact]
        public void Should_Not_Be_Able_To_Add_Preferred_Number_Of_Votes_Greater_Than_Respondents()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(NonEmptyString.Create(topic), numberOfRespondents, NonEmptyString.Create(respondentType));

            survey.AddSurveyOption(NonEmptyString.Create("Tabs"), 1);

            Assert.Throws<SurveyDomainException>(() =>
            {
                survey.AddSurveyOption(NonEmptyString.Create("Spaces"), 1001);
            });
        }

        [Fact]
        public void Should_Not_Be_Able_To_Add_Preferred_Number_Of_Votes_If_Total_Exceeds_Respondents()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(NonEmptyString.Create(topic), numberOfRespondents, NonEmptyString.Create(respondentType));

            survey.AddSurveyOption(NonEmptyString.Create("Tabs"), 500);

            Assert.Throws<SurveyDomainException>(() =>
            {
                survey.AddSurveyOption(NonEmptyString.Create("Spaces"), 501);
            });
        }

        [Fact]
        public void Creating_Survey_Should_Add_SurveyCreated_Event_To_DomainEvents()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1;
            const string respondentType = "Developers";

            var survey = new Survey(NonEmptyString.Create(topic), numberOfRespondents, NonEmptyString.Create(respondentType));

            survey.AddSurveyOption(NonEmptyString.Create("Tabs"));
            survey.AddSurveyOption(NonEmptyString.Create("Spaces"));


            var surveyCreatedEvent = survey.DomainEvents.First() as SurveyCreatedDomainEvent;

            Assert.True(surveyCreatedEvent?.Survey == survey);
        }
    }
}
