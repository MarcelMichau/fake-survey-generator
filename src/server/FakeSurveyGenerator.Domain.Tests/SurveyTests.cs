using System;
using System.Linq;
using AutoFixture;
using AutoFixture.Idioms;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;
using FakeSurveyGenerator.Domain.Common;
using FakeSurveyGenerator.Domain.DomainEvents;
using FakeSurveyGenerator.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace FakeSurveyGenerator.Domain.Tests;

public sealed class SurveyTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void GivenValidSurveyDetail_WhenCreatingSurvey_ThenValidSurveyIsCreated()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.Topic.Value.Should().Be(topic);
        survey.NumberOfRespondents.Should().Be(numberOfRespondents);
        survey.RespondentType.Value.Should().Be(respondentType);
    }

    [Fact]
    public void GivenSomeEmptyFields_WhenCreatingSurvey_ThenExceptionShouldBeThrown()
    {
        var assertion = _fixture.Create<GuardClauseAssertion>();
        assertion.Verify(typeof(Survey).GetConstructors());
    }

    [Fact]
    public void GivenNoRespondents_WhenCreatingSurvey_ThenSurveyDomainExceptionShouldBeThrown()
    {
        var topic = _fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 0;
        var respondentType = _fixture.Create<NonEmptyString>();

        Action act = () => { _ = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType); };

        act.Should().ThrowExactly<SurveyDomainException>();
    }

    [Fact]
    public void GivenValidOption_WhenAddingOptionToSurvey_ShouldAddOptionToSurveyOptionsList()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var option1 = _fixture.Create<NonEmptyString>();
        var option2 = _fixture.Create<NonEmptyString>();

        survey.AddSurveyOption(option1);
        survey.AddSurveyOption(option2);

        survey.Options.Count.Should().Be(2);
        survey.Options[0].OptionText.Value.Should().Be(option1);
        survey.Options[^1].OptionText.Value.Should().Be(option2);
    }

    [Fact]
    public void GivenEmptyOption_WhenAddingOptionToSurvey_ThenExceptionShouldBeThrown()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        Action act = () =>
        {
            var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption(NonEmptyString.Create(""));
        };

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void GivenSurveyWithNoOption_WhenCalculatingOutcome_ThenSurveyDomainExceptionShouldBeThrown()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        Action act = () =>
        {
            var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

            survey.CalculateOutcome();
        };

        act.Should().ThrowExactly<SurveyDomainException>();
    }

    [Fact]
    public void GivenSurveyWithOptions_WhenCalculatingOutcome_ThenOptionNumberOfVotesShouldBeDistributedEvenly()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());

        survey.CalculateOutcome();

        survey.Options.Sum(option => option.NumberOfVotes).Should().Be(numberOfRespondents);
        survey.Options.All(option => option.NumberOfVotes > 0).Should().BeTrue();
    }

    [Fact]
    public void GivenSurveyWithOptions_WhenCalculatingOneSidedOutcome_ThenOneOptionShouldReceiveAllVotes()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());

        survey.CalculateOneSidedOutcome();

        survey.Options.Max(option => option.NumberOfVotes).Should().Be(numberOfRespondents);
    }

    [Fact]
    public void
        GivenSurveyWithOptionsHavingPreferredNumberOfVotes_WhenCalculatingOutcome_ThenEachOptionShouldHaveExpectedPreferredNumberOfVotes()
    {
        var topic = _fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 1000;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), 600);
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), 400);

        survey.CalculateOutcome();

        survey.Options[0].NumberOfVotes.Should().Be(600);
        survey.Options[^1].NumberOfVotes.Should().Be(400);
    }

    [Fact]
    public void
        GivenSurveyWithOneOptionHavingPreferredNumberOfVotesExceedingTotalRespondents_WhenAddingOptionToSurvey_ThenSurveyDomainExceptionShouldBeThrown()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), numberOfRespondents - 1);

        Action act = () => { survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), numberOfRespondents + 1); };

        act.Should().ThrowExactly<SurveyDomainException>();
    }

    [Fact]
    public void
        GivenSurveyWithOptionsWhereCombinedPreferredNumberOfVotesExceedTotalRespondents_WhenAddingOptionToSurvey_ThenSurveyDomainExceptionShouldBeThrown()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), numberOfRespondents);

        Action act = () => { survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), numberOfRespondents + 1); };

        act.Should().ThrowExactly<SurveyDomainException>();
    }

    [Fact]
    public void
        GivenValidSurveyDetail_WhenCreatingSurvey_ThenValidSurveyIsCreatedWithSurveyCreatedEventAddedToDomainEvents()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());

        survey.DomainEvents.Should().AllBeOfType<SurveyCreatedDomainEvent>();

        var surveyCreatedEvent = (SurveyCreatedDomainEvent)survey.DomainEvents.First();

        surveyCreatedEvent.Survey.Should().Be(survey);
    }
}