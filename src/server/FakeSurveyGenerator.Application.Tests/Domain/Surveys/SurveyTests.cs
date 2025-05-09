using AutoFixture;
using AutoFixture.Idioms;
using EnumerableAsyncProcessor.Extensions;
using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Domain.Users;
using TUnit.Assertions.AssertConditions.Throws;

namespace FakeSurveyGenerator.Application.Tests.Domain.Surveys;

public sealed class SurveyTests
{
    private readonly Fixture _fixture = new();

    [Test]
    public async Task GivenValidSurveyDetail_WhenCreatingSurvey_ThenValidSurveyIsCreated()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        await Assert.That(survey.Topic.Value).IsEqualTo(topic);
        await Assert.That(survey.NumberOfRespondents).IsEqualTo(numberOfRespondents);
        await Assert.That(survey.RespondentType.Value).IsEqualTo(respondentType);
    }

    [Test]
    public void GivenSomeEmptyFields_WhenCreatingSurvey_ThenExceptionShouldBeThrown()
    {
        var assertion = _fixture.Create<GuardClauseAssertion>();
        assertion.Verify(typeof(Survey).GetConstructors());
    }

    [Test]
    public async Task GivenNoRespondents_WhenCreatingSurvey_ThenSurveyDomainExceptionShouldBeThrown()
    {
        var topic = _fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 0;
        var respondentType = _fixture.Create<NonEmptyString>();

        await Assert.That(() =>
        {
            _ = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    [Test]
    public async Task GivenValidOption_WhenAddingOptionToSurvey_ShouldAddOptionToSurveyOptionsList()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var option1 = _fixture.Create<NonEmptyString>();
        var option2 = _fixture.Create<NonEmptyString>();

        survey.AddSurveyOption(option1);
        survey.AddSurveyOption(option2);

        await Assert.That(survey.Options.Count).IsEqualTo(2);
        await Assert.That(survey.Options[0].OptionText.Value).IsEqualTo(option1);
        await Assert.That(survey.Options[^1].OptionText.Value).IsEqualTo(option2);
    }

    [Test]
    public async Task GivenEmptyOption_WhenAddingOptionToSurvey_ThenExceptionShouldBeThrown()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        await Assert.That(() =>
        {
            var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

            survey.AddSurveyOption(NonEmptyString.Create(""));
        }).ThrowsException();
    }

    [Test]
    public async Task GivenDuplicateOption_WhenAddingOptionToSurvey_ThenSurveyDomainExceptionShouldBeThrown()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        
        // Use a specific value to ensure we can create an exact duplicate
        var optionText = NonEmptyString.Create("Duplicate Option");
        
        // Add the first option
        survey.AddSurveyOption(optionText);
        
        // Act & Assert - Adding the same option should throw
        await Assert.That(() =>
        {
            survey.AddSurveyOption(optionText);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>().And.HasMessageEqualTo("Duplicate survey option.");
    }
    
    [Test]
    public async Task GivenDuplicateOptionWithDifferentCase_WhenAddingOptionToSurvey_ThenSurveyDomainExceptionShouldBeThrown()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        
        // Add the first option in lowercase
        survey.AddSurveyOption(NonEmptyString.Create("duplicate case insensitive"));
        
        // Act & Assert - Adding the same option in different case should throw
        await Assert.That(() =>
        {
            survey.AddSurveyOption(NonEmptyString.Create("DUPLICATE CASE INSENSITIVE"));
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>().And.HasMessageEqualTo("Duplicate survey option.");
    }
    
    [Test]
    public async Task GivenDuplicateOptionWithPreferredVotes_WhenAddingOptionToSurvey_ThenSurveyDomainExceptionShouldBeThrown()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        
        // Add the first option
        var optionText = NonEmptyString.Create("Option with votes");
        survey.AddSurveyOption(optionText);
        
        // Act & Assert - Adding the same option with preferred votes should also throw
        await Assert.That(() =>
        {
            survey.AddSurveyOption(optionText, 50);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>().HasMessageEqualTo("Duplicate survey option.");
    }

    [Test]
    public async Task GivenNullOptionCollection_WhenAddingSurveyOptions_ThenArgumentNullExceptionShouldBeThrown()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        
        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOptions(null!);
        }).ThrowsException().And.IsTypeOf<ArgumentNullException>();
    }
    
    [Test]
    public async Task GivenEmptyOptionCollection_WhenAddingSurveyOptions_ThenNoOptionsShouldBeAdded()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        
        // Act
        survey.AddSurveyOptions(Enumerable.Empty<SurveyOption>());
        
        // Assert
        await Assert.That(survey.Options.Count).IsEqualTo(0);
    }
    
    [Test]
    public async Task GivenValidOptionCollection_WhenAddingSurveyOptions_ThenAllOptionsShouldBeAdded()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        
        var options = new List<SurveyOption>
        {
            new(NonEmptyString.Create("Option 1"), 30),
            new(NonEmptyString.Create("Option 2"), 20),
            new(NonEmptyString.Create("Option 3"), 50)
        };
        
        // Act
        survey.AddSurveyOptions(options);
        
        // Assert
        await Assert.That(survey.Options.Count).IsEqualTo(3);
        await Assert.That(survey.Options.Sum(o => o.PreferredNumberOfVotes)).IsEqualTo(100);
    }

    [Test]
    public async Task GivenSurveyWithNoOption_WhenCalculatingOutcome_ThenSurveyDomainExceptionShouldBeThrown()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        await Assert.That(() =>
        {
            var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

            survey.CalculateOutcome();
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    [Test]
    public async Task GivenSurveyWithOptions_WhenCalculatingOutcome_ThenOptionNumberOfVotesShouldBeDistributedEvenly()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());

        survey.CalculateOutcome();

        await Assert.That(survey.Options.Sum(option => option.NumberOfVotes)).IsEqualTo(numberOfRespondents);
    }

    [Test]
    public async Task GivenSurveyWithOptions_WhenCalculatingOneSidedOutcome_ThenOneOptionShouldReceiveAllVotes()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());

        survey.CalculateOneSidedOutcome();

        await Assert.That(survey.Options.Max(option => option.NumberOfVotes)).IsEqualTo(numberOfRespondents);
    }

    [Test]
    public async Task
        GivenSurveyWithOptionsHavingPreferredNumberOfVotes_WhenCalculatingOutcome_ThenEachOptionShouldHaveExpectedPreferredNumberOfVotes()
    {
        var topic = _fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 1000;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), 600);
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), 400);

        survey.CalculateOutcome();

        await Assert.That(survey.Options[0].NumberOfVotes).IsEqualTo(600);
        await Assert.That(survey.Options[^1].NumberOfVotes).IsEqualTo(400);

        await Assert.That(survey.IsRigged).IsTrue();
    }

    [Test]
    public async Task
        GivenSurveyWithOneOptionHavingPreferredNumberOfVotesExceedingTotalRespondents_WhenAddingOptionToSurvey_ThenSurveyDomainExceptionShouldBeThrown()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), numberOfRespondents - 1);

        await Assert.That(() =>
        {
            survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), numberOfRespondents + 1);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    [Test]
    public async Task
        GivenSurveyWithOptionsWhereCombinedPreferredNumberOfVotesExceedTotalRespondents_WhenAddingOptionToSurvey_ThenSurveyDomainExceptionShouldBeThrown()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), numberOfRespondents);

        await Assert.That(() =>
        {
            survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), numberOfRespondents + 1);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    [Test]
    public async Task
        GivenValidSurveyDetail_WhenCreatingSurvey_ThenValidSurveyIsCreatedWithSurveyCreatedEventAddedToDomainEvents()
    {
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());

        survey.DomainEvents.ForEachAsync(async surveyDomainEvent =>
        {
            await Assert.That(surveyDomainEvent).IsTypeOf<SurveyCreatedDomainEvent>();
        });

        var surveyCreatedEvent = (SurveyCreatedDomainEvent)survey.DomainEvents.First();

        await Assert.That(surveyCreatedEvent.Survey).IsEqualTo(survey);
    }
}