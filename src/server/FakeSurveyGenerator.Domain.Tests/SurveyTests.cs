using System;
using System.Linq;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;
using FakeSurveyGenerator.Domain.Common;
using FakeSurveyGenerator.Domain.DomainEvents;
using FakeSurveyGenerator.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FakeSurveyGenerator.Domain.Tests
{
    public sealed class SurveyTests
    {
        private readonly User _testUser = new User(NonEmptyString.Create("Test User"),
            NonEmptyString.Create("test.user@test.com"), NonEmptyString.Create("test-id"));

        [Fact]
        public void GivenValidSurveyDetail_WhenCreatingSurvey_ThenValidSurveyIsCreated()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1;
            const string respondentType = "Developers";

            var survey = new Survey(_testUser, NonEmptyString.Create(topic), numberOfRespondents,
                NonEmptyString.Create(respondentType));

            survey.Topic.Value.Should().Be(topic);
            survey.NumberOfRespondents.Should().Be(numberOfRespondents);
            survey.RespondentType.Value.Should().Be(respondentType);
        }

        [Fact]
        public void GivenNoUser_WhenCreatingSurvey_ThenExceptionShouldBeThrown()
        {
            const string topic = "";
            const int numberOfRespondents = 1;
            const string respondentType = "Developers";

            Action act = () =>
            {
                _ = new Survey(null, NonEmptyString.Create(topic), numberOfRespondents,
                    NonEmptyString.Create(respondentType));
            };

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void GivenNoTopic_WhenCreatingSurvey_ThenExceptionShouldBeThrown()
        {
            const string topic = "";
            const int numberOfRespondents = 1;
            const string respondentType = "Developers";

            Action act = () =>
            {
                _ = new Survey(_testUser, NonEmptyString.Create(topic), numberOfRespondents,
                    NonEmptyString.Create(respondentType));
            };

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void GivenNoRespondents_WhenCreatingSurvey_ThenSurveyDomainExceptionShouldBeThrown()
        {
            const string topic = "To be, or not to be?";
            const int numberOfRespondents = 0;
            const string respondentType = "Writers";

            Action act = () =>
            {
                _ = new Survey(_testUser, NonEmptyString.Create(topic),
                    numberOfRespondents, NonEmptyString.Create(respondentType));
            };

            act.Should().ThrowExactly<SurveyDomainException>();
        }

        [Fact]
        public void GivenNoRespondentType_WhenCreatingSurvey_ThenExceptionShouldBeThrown()
        {
            const string topic = "To be, or not to be?";
            const int numberOfRespondents = 1;
            const string respondentType = "";

            Action act = () =>
            {
                _ = new Survey(_testUser, NonEmptyString.Create(topic), numberOfRespondents,
                    NonEmptyString.Create(respondentType));
            };

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void GivenValidOption_WhenAddingOptionToSurvey_ShouldAddOptionToSurveyOptionsList()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1;
            const string respondentType = "Developers";

            var survey = new Survey(_testUser, NonEmptyString.Create(topic), numberOfRespondents,
                NonEmptyString.Create(respondentType));

            survey.AddSurveyOption(NonEmptyString.Create("Tabs"));
            survey.AddSurveyOption(NonEmptyString.Create("Spaces"));

            survey.Options.Count.Should().Be(2);
            survey.Options.First().OptionText.Value.Should().Be("Tabs");
            survey.Options.Last().OptionText.Value.Should().Be("Spaces");
        }

        [Fact]
        public void GivenEmptyOption_WhenAddingOptionToSurvey_ThenExceptionShouldBeThrown()
        {
            const string topic = "To be, or not to be?";
            const int numberOfRespondents = 2;
            const string respondentType = "Writers";

            Action act = () =>
            {
                var survey = new Survey(_testUser, NonEmptyString.Create(topic), numberOfRespondents,
                    NonEmptyString.Create(respondentType));

                survey.AddSurveyOption(NonEmptyString.Create(""));
            };

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void GivenSurveyWithNoOption_WhenCalculatingOutcome_ThenSurveyDomainExceptionShouldBeThrown()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            Action act = () =>
            {
                var survey = new Survey(_testUser, NonEmptyString.Create(topic), numberOfRespondents,
                    NonEmptyString.Create(respondentType));

                survey.CalculateOutcome();
            };

            act.Should().ThrowExactly<SurveyDomainException>();
        }

        [Fact]
        public void GivenSurveyWithOptions_WhenCalculatingOutcome_ThenOptionNumberOfVotesShouldBeDistributedEvenly()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(_testUser, NonEmptyString.Create(topic), numberOfRespondents,
                NonEmptyString.Create(respondentType));

            survey.AddSurveyOption(NonEmptyString.Create("Tabs"));
            survey.AddSurveyOption(NonEmptyString.Create("Spaces"));

            survey.CalculateOutcome();

            survey.Options.Sum(option => option.NumberOfVotes).Should().Be(numberOfRespondents);
            survey.Options.All(option => option.NumberOfVotes > 0).Should().BeTrue();
        }

        [Fact]
        public void GivenSurveyWithOptions_WhenCalculatingOneSidedOutcome_ThenOneOptionShouldReceiveAllVotes()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(_testUser, NonEmptyString.Create(topic), numberOfRespondents,
                NonEmptyString.Create(respondentType));

            survey.AddSurveyOption(NonEmptyString.Create("Tabs"));
            survey.AddSurveyOption(NonEmptyString.Create("Spaces"));

            survey.CalculateOneSidedOutcome();

            survey.Options.Max(option => option.NumberOfVotes).Should().Be(numberOfRespondents);
        }

        [Fact]
        public void
            GivenSurveyWithOptionsHavingPreferredNumberOfVotes_WhenCalculatingOutcome_ThenEachOptionShouldHaveExpectedPreferredNumberOfVotes()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(_testUser, NonEmptyString.Create(topic), numberOfRespondents,
                NonEmptyString.Create(respondentType));

            survey.AddSurveyOption(NonEmptyString.Create("Tabs"), 600);
            survey.AddSurveyOption(NonEmptyString.Create("Spaces"), 400);

            survey.CalculateOutcome();

            survey.Options.First().NumberOfVotes.Should().Be(600);
            survey.Options.Last().NumberOfVotes.Should().Be(400);
        }

        [Fact]
        public void
            GivenSurveyWithOneOptionHavingPreferredNumberOfVotesExceedingTotalRespondents_WhenAddingOptionToSurvey_ThenSurveyDomainExceptionShouldBeThrown()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(_testUser, NonEmptyString.Create(topic), numberOfRespondents,
                NonEmptyString.Create(respondentType));

            survey.AddSurveyOption(NonEmptyString.Create("Tabs"), 1);

            Action act = () => { survey.AddSurveyOption(NonEmptyString.Create("Spaces"), 1001); };

            act.Should().ThrowExactly<SurveyDomainException>();
        }

        [Fact]
        public void
            GivenSurveyWithOptionsWhereCombinedPreferredNumberOfVotesExceedTotalRespondents_WhenAddingOptionToSurvey_ThenSurveyDomainExceptionShouldBeThrown()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1000;
            const string respondentType = "Developers";

            var survey = new Survey(_testUser, NonEmptyString.Create(topic), numberOfRespondents,
                NonEmptyString.Create(respondentType));

            survey.AddSurveyOption(NonEmptyString.Create("Tabs"), 500);

            Action act = () => { survey.AddSurveyOption(NonEmptyString.Create("Spaces"), 501); };

            act.Should().ThrowExactly<SurveyDomainException>();
        }

        [Fact]
        public void
            GivenValidSurveyDetail_WhenCreatingSurvey_ThenValidSurveyIsCreatedWithSurveyCreatedEventAddedToDomainEvents()
        {
            const string topic = "Tabs or spaces?";
            const int numberOfRespondents = 1;
            const string respondentType = "Developers";

            var survey = new Survey(_testUser, NonEmptyString.Create(topic), numberOfRespondents,
                NonEmptyString.Create(respondentType));

            survey.AddSurveyOption(NonEmptyString.Create("Tabs"));
            survey.AddSurveyOption(NonEmptyString.Create("Spaces"));

            survey.DomainEvents.Should().AllBeOfType<SurveyCreatedDomainEvent>();

            var surveyCreatedEvent = (SurveyCreatedDomainEvent) survey.DomainEvents.First();

            surveyCreatedEvent.Survey.Should().Be(survey);
        }
    }
}