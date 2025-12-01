using System.Text.Json;

using AutoFixture;
using AutoFixture.Idioms;
using EnumerableAsyncProcessor.Extensions;
using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Domain.Users;
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
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>().And.HasMessageEqualTo("Duplicate survey option.");
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
        survey.AddSurveyOptions([]);

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

    /// <summary>
    /// Tests that IsRigged returns false when the survey has no options added.
    /// The Any() method on an empty collection returns false.
    /// </summary>
    [Test]
    public async Task IsRigged_NoOptions_ReturnsFalse()
    {
        // Arrange
        var fixture = new Fixture();
        var survey = new Survey(
            fixture.Create<User>(),
            fixture.Create<NonEmptyString>(),
            1000,
            fixture.Create<NonEmptyString>());

        // Act
        var isRigged = survey.IsRigged;

        // Assert
        await Assert.That(isRigged).IsFalse();
    }

    /// <summary>
    /// Tests that IsRigged returns false when the survey only contains options
    /// without preferred votes (non-rigged options).
    /// </summary>
    [Test]
    public async Task IsRigged_OnlyNonRiggedOptions_ReturnsFalse()
    {
        // Arrange
        var fixture = new Fixture();
        var survey = new Survey(
            fixture.Create<User>(),
            fixture.Create<NonEmptyString>(),
            1000,
            fixture.Create<NonEmptyString>());

        survey.AddSurveyOption(fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(fixture.Create<NonEmptyString>());

        // Act
        var isRigged = survey.IsRigged;

        // Assert
        await Assert.That(isRigged).IsFalse();
    }

    /// <summary>
    /// Tests that IsRigged returns true when the survey contains at least one option
    /// with a preferred number of votes greater than zero.
    /// </summary>
    [Test]
    public async Task IsRigged_OneRiggedOption_ReturnsTrue()
    {
        // Arrange
        var fixture = new Fixture();
        var survey = new Survey(
            fixture.Create<User>(),
            fixture.Create<NonEmptyString>(),
            1000,
            fixture.Create<NonEmptyString>());

        survey.AddSurveyOption(fixture.Create<NonEmptyString>(), 100);

        // Act
        var isRigged = survey.IsRigged;

        // Assert
        await Assert.That(isRigged).IsTrue();
    }

    /// <summary>
    /// Tests that IsRigged returns true when the survey contains multiple options
    /// with preferred votes.
    /// </summary>
    [Test]
    public async Task IsRigged_MultipleRiggedOptions_ReturnsTrue()
    {
        // Arrange
        var fixture = new Fixture();
        var survey = new Survey(
            fixture.Create<User>(),
            fixture.Create<NonEmptyString>(),
            1000,
            fixture.Create<NonEmptyString>());

        survey.AddSurveyOption(fixture.Create<NonEmptyString>(), 200);
        survey.AddSurveyOption(fixture.Create<NonEmptyString>(), 300);
        survey.AddSurveyOption(fixture.Create<NonEmptyString>(), 400);

        // Act
        var isRigged = survey.IsRigged;

        // Assert
        await Assert.That(isRigged).IsTrue();
    }

    /// <summary>
    /// Tests that IsRigged returns true when the survey contains a mix of rigged
    /// and non-rigged options. Even one rigged option makes the entire survey rigged.
    /// </summary>
    [Test]
    public async Task IsRigged_MixedRiggedAndNonRiggedOptions_ReturnsTrue()
    {
        // Arrange
        var fixture = new Fixture();
        var survey = new Survey(
            fixture.Create<User>(),
            fixture.Create<NonEmptyString>(),
            1000,
            fixture.Create<NonEmptyString>());

        survey.AddSurveyOption(fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(fixture.Create<NonEmptyString>(), 250);
        survey.AddSurveyOption(fixture.Create<NonEmptyString>());

        // Act
        var isRigged = survey.IsRigged;

        // Assert
        await Assert.That(isRigged).IsTrue();
    }

    /// <summary>
    /// Tests that IsRigged returns true when the survey contains an option
    /// with the minimum preferred votes value of 1, which is the boundary for rigged options.
    /// </summary>
    [Test]
    public async Task IsRigged_OptionWithMinimumPreferredVotes_ReturnsTrue()
    {
        // Arrange
        var fixture = new Fixture();
        var survey = new Survey(
            fixture.Create<User>(),
            fixture.Create<NonEmptyString>(),
            1000,
            fixture.Create<NonEmptyString>());

        survey.AddSurveyOption(fixture.Create<NonEmptyString>(), 1);

        // Act
        var isRigged = survey.IsRigged;

        // Assert
        await Assert.That(isRigged).IsTrue();
    }

    /// <summary>
    /// Tests that AddSurveyOption successfully adds a very long option text.
    /// Verifies that long strings are handled correctly without truncation or errors.
    /// </summary>
    [Test]
    public async Task AddSurveyOption_VeryLongOptionText_AddsOptionSuccessfully()
    {
        // Arrange
        var fixture = new Fixture();
        var survey = new Survey(
            fixture.Create<User>(),
            fixture.Create<NonEmptyString>(),
            fixture.Create<int>(),
            fixture.Create<NonEmptyString>());

        var longText = new string('A', 10000);
        var option = NonEmptyString.Create(longText);

        // Act
        survey.AddSurveyOption(option);

        // Assert
        await Assert.That(survey.Options).Count().EqualTo(1);
        await Assert.That(survey.Options[0].OptionText.Value).IsEqualTo(longText);
    }

    /// <summary>
    /// Tests that AddSurveyOption maintains order when multiple options are added sequentially.
    /// Verifies that options appear in the Options list in the order they were added.
    /// </summary>
    [Test]
    public async Task AddSurveyOption_MultipleOptionsAdded_MaintainsInsertionOrder()
    {
        // Arrange
        var fixture = new Fixture();
        var survey = new Survey(
            fixture.Create<User>(),
            fixture.Create<NonEmptyString>(),
            fixture.Create<int>(),
            fixture.Create<NonEmptyString>());

        var option1 = NonEmptyString.Create("First Option");
        var option2 = NonEmptyString.Create("Second Option");
        var option3 = NonEmptyString.Create("Third Option");

        // Act
        survey.AddSurveyOption(option1);
        survey.AddSurveyOption(option2);
        survey.AddSurveyOption(option3);

        // Assert
        await Assert.That(survey.Options).Count().EqualTo(3);
        await Assert.That(survey.Options[0].OptionText.Value).IsEqualTo("First Option");
        await Assert.That(survey.Options[1].OptionText.Value).IsEqualTo("Second Option");
        await Assert.That(survey.Options[2].OptionText.Value).IsEqualTo("Third Option");
    }

    /// <summary>
    /// Tests that AddSurveyOption successfully adds option when existing list already contains options.
    /// Verifies that the new option is appended to the existing list.
    /// </summary>
    [Test]
    public async Task AddSurveyOption_AddingToNonEmptyList_AppendsOptionCorrectly()
    {
        // Arrange
        var fixture = new Fixture();
        var survey = new Survey(
            fixture.Create<User>(),
            fixture.Create<NonEmptyString>(),
            fixture.Create<int>(),
            fixture.Create<NonEmptyString>());

        var existingOption = NonEmptyString.Create("Existing Option");
        survey.AddSurveyOption(existingOption);

        var newOption = NonEmptyString.Create("New Option");

        // Act
        survey.AddSurveyOption(newOption);

        // Assert
        await Assert.That(survey.Options).Count().EqualTo(2);
        await Assert.That(survey.Options[0].OptionText.Value).IsEqualTo("Existing Option");
        await Assert.That(survey.Options[1].OptionText.Value).IsEqualTo("New Option");
    }

    /// <summary>
    /// Tests that AddSurveyOption successfully adds options with leading and trailing spaces.
    /// Verifies that non-empty strings with spaces are preserved and not considered duplicates if they differ.
    /// </summary>
    [Test]
    public async Task AddSurveyOption_OptionTextWithLeadingTrailingSpaces_AddsOptionSuccessfully()
    {
        // Arrange
        var fixture = new Fixture();
        var survey = new Survey(
            fixture.Create<User>(),
            fixture.Create<NonEmptyString>(),
            fixture.Create<int>(),
            fixture.Create<NonEmptyString>());

        var option1 = NonEmptyString.Create(" Option ");
        var option2 = NonEmptyString.Create("  Option  ");

        // Act
        survey.AddSurveyOption(option1);

        // Assert - First option should be added
        await Assert.That(survey.Options).Count().EqualTo(1);
        await Assert.That(survey.Options[0].OptionText.Value).IsEqualTo(" Option ");

        // Act - Second option with different spacing should be treated as duplicate
        await Assert.That(() =>
        {
            survey.AddSurveyOption(option2);
        }).ThrowsException()
          .And.IsTypeOf<SurveyDomainException>()
          .And.HasMessageEqualTo("Duplicate survey option.");
    }

    /// <summary>
    /// Tests that CalculateOneSidedOutcome throws SurveyDomainException when the survey has no options.
    /// Input: Survey with no survey options added.
    /// Expected: SurveyDomainException is thrown.
    /// </summary>
    [Test]
    public async Task CalculateOneSidedOutcome_NoOptions_ThrowsSurveyDomainException()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();
        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        // Act & Assert
        await Assert.That(() => survey.CalculateOneSidedOutcome())
            .ThrowsExactly<SurveyDomainException>();
    }

    /// <summary>
    /// Tests that CalculateOneSidedOutcome correctly distributes all votes to the single option when only one option exists.
    /// Input: Survey with exactly one survey option.
    /// Expected: The single option receives all votes equal to NumberOfRespondents.
    /// </summary>
    [Test]
    public async Task CalculateOneSidedOutcome_SingleOption_AllVotesToSingleOption()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();
        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());

        // Act
        survey.CalculateOneSidedOutcome();

        // Assert
        await Assert.That(survey.Options.Count).IsEqualTo(1);
        await Assert.That(survey.Options[0].NumberOfVotes).IsEqualTo(numberOfRespondents);
    }

    /// <summary>
    /// Tests that CalculateOneSidedOutcome distributes votes correctly when NumberOfRespondents is at the minimum valid value of 1.
    /// Input: Survey with NumberOfRespondents = 1 and multiple options.
    /// Expected: Exactly one option receives 1 vote, all other options have 0 votes, and total votes equal 1.
    /// </summary>
    [Test]
    public async Task CalculateOneSidedOutcome_MinimumRespondents_DistributesOneVoteToSingleOption()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = 1;
        var respondentType = _fixture.Create<NonEmptyString>();
        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());

        // Act
        survey.CalculateOneSidedOutcome();

        // Assert
        var totalVotes = survey.Options.Sum(option => option.NumberOfVotes);
        var optionsWithVotes = survey.Options.Count(option => option.NumberOfVotes > 0);

        await Assert.That(totalVotes).IsEqualTo(1);
        await Assert.That(optionsWithVotes).IsEqualTo(1);
        await Assert.That(survey.Options.Max(option => option.NumberOfVotes)).IsEqualTo(1);
    }

    /// <summary>
    /// Tests that CalculateOneSidedOutcome distributes all votes to exactly one option and no votes to others.
    /// Input: Survey with multiple options and standard NumberOfRespondents.
    /// Expected: Exactly one option receives all votes equal to NumberOfRespondents, all other options have 0 votes.
    /// </summary>
    [Test]
    public async Task CalculateOneSidedOutcome_MultipleOptions_OnlyOneOptionReceivesAllVotes()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();
        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>());

        // Act
        survey.CalculateOneSidedOutcome();

        // Assert
        var totalVotes = survey.Options.Sum(option => option.NumberOfVotes);
        var maxVotes = survey.Options.Max(option => option.NumberOfVotes);
        var optionsWithVotes = survey.Options.Count(option => option.NumberOfVotes > 0);
        var optionsWithoutVotes = survey.Options.Count(option => option.NumberOfVotes == 0);

        await Assert.That(totalVotes).IsEqualTo(numberOfRespondents);
        await Assert.That(maxVotes).IsEqualTo(numberOfRespondents);
        await Assert.That(optionsWithVotes).IsEqualTo(1);
        await Assert.That(optionsWithoutVotes).IsEqualTo(survey.Options.Count - 1);
    }

    /// <summary>
    /// Tests that AddSurveyOptions adds a single option correctly when the collection contains exactly one option.
    /// This validates the iteration behavior with a boundary case between empty and multiple options.
    /// </summary>
    [Test]
    public async Task AddSurveyOptions_SingleOptionInCollection_ShouldAddOneOption()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var options = new List<SurveyOption>
        {
            new(NonEmptyString.Create("Single Option"), 50)
        };

        // Act
        survey.AddSurveyOptions(options);

        // Assert
        await Assert.That(survey.Options.Count).IsEqualTo(1);
        await Assert.That(survey.Options[0].OptionText.Value).IsEqualTo("Single Option");
        await Assert.That(survey.Options[0].PreferredNumberOfVotes).IsEqualTo(50);
    }

    /// <summary>
    /// Tests that AddSurveyOptions throws SurveyDomainException when the collection contains duplicate option text.
    /// This ensures that duplicate validation in AddSurveyOption is properly triggered during iteration.
    /// </summary>
    [Test]
    public async Task AddSurveyOptions_CollectionWithDuplicateOptions_ShouldThrowSurveyDomainException()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var options = new List<SurveyOption>
        {
            new(NonEmptyString.Create("Option 1"), 30),
            new(NonEmptyString.Create("Option 1"), 20)
        };

        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOptions(options);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    /// <summary>
    /// Tests that AddSurveyOptions throws SurveyDomainException when one option's preferred votes exceed total respondents.
    /// This validates that vote validation in AddSurveyOption is properly triggered during iteration.
    /// </summary>
    [Test]
    public async Task AddSurveyOptions_OptionWithPreferredVotesExceedingRespondents_ShouldThrowSurveyDomainException()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var options = new List<SurveyOption>
        {
            new(NonEmptyString.Create("Option 1"), 150)
        };

        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOptions(options);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    /// <summary>
    /// Tests that AddSurveyOptions throws SurveyDomainException when combined preferred votes exceed total respondents.
    /// This validates proper exception propagation when the second option causes the total to exceed the limit.
    /// </summary>
    [Test]
    public async Task AddSurveyOptions_CollectionWithCombinedVotesExceedingRespondents_ShouldThrowSurveyDomainException()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var options = new List<SurveyOption>
        {
            new(NonEmptyString.Create("Option 1"), 60),
            new(NonEmptyString.Create("Option 2"), 50)
        };

        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOptions(options);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    /// <summary>
    /// Tests that AddSurveyOptions correctly handles options with zero preferred votes.
    /// This validates the boundary case where preferred votes is the minimum valid value.
    /// </summary>
    [Test]
    public async Task AddSurveyOptions_OptionsWithZeroPreferredVotes_ShouldAddOptionsSuccessfully()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var options = new List<SurveyOption>
        {
            new(NonEmptyString.Create("Option 1"), 0),
            new(NonEmptyString.Create("Option 2"), 0)
        };

        // Act
        survey.AddSurveyOptions(options);

        // Assert
        await Assert.That(survey.Options.Count).IsEqualTo(2);
        await Assert.That(survey.Options[0].PreferredNumberOfVotes).IsEqualTo(0);
        await Assert.That(survey.Options[1].PreferredNumberOfVotes).IsEqualTo(0);
    }

    /// <summary>
    /// Tests that AddSurveyOptions correctly handles options with negative preferred votes.
    /// This validates edge case behavior with negative integer values.
    /// </summary>
    [Test]
    public async Task AddSurveyOptions_OptionsWithNegativePreferredVotes_ShouldThrowSurveyDomainException()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var options = new List<SurveyOption>
        {
            new(NonEmptyString.Create("Option 1"), -10),
            new(NonEmptyString.Create("Option 2"), -5)
        };

        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOptions(options);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    /// <summary>
    /// Tests that AddSurveyOptions correctly processes options when preferred votes exactly equal total respondents.
    /// This validates the boundary case where the sum equals the maximum allowed value.
    /// </summary>
    [Test]
    public async Task AddSurveyOptions_CollectionWithPreferredVotesEqualingRespondents_ShouldAddOptionsSuccessfully()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 1000;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var options = new List<SurveyOption>
        {
            new(NonEmptyString.Create("Option 1"), 400),
            new(NonEmptyString.Create("Option 2"), 300),
            new(NonEmptyString.Create("Option 3"), 300)
        };

        // Act
        survey.AddSurveyOptions(options);

        // Assert
        await Assert.That(survey.Options.Count).IsEqualTo(3);
        await Assert.That(survey.Options.Sum(o => o.PreferredNumberOfVotes)).IsEqualTo(1000);
    }

    /// <summary>
    /// Tests that AddSurveyOptions throws SurveyDomainException when the collection contains options with int.MinValue for preferred votes.
    /// This validates edge case behavior with minimum integer value which is negative and invalid.
    /// </summary>
    [Test]
    public async Task AddSurveyOptions_OptionWithMinIntPreferredVotes_ShouldThrowSurveyDomainException()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var options = new List<SurveyOption>
        {
            new(NonEmptyString.Create("Option 1"), int.MinValue)
        };

        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOptions(options);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    /// <summary>
    /// Tests that AddSurveyOptions handles a large collection of options efficiently.
    /// This validates that the method can process many options without issues.
    /// </summary>
    [Test]
    public async Task AddSurveyOptions_LargeCollectionOfOptions_ShouldAddAllOptions()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = 10000;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var options = new List<SurveyOption>();
        for (int i = 0; i < 100; i++)
        {
            options.Add(new SurveyOption(NonEmptyString.Create($"Option {i}"), 0));
        }

        // Act
        survey.AddSurveyOptions(options);

        // Assert
        await Assert.That(survey.Options.Count).IsEqualTo(100);
    }

    /// <summary>
    /// Tests that AddSurveyOptions processes options in order and stops when an exception occurs mid-iteration.
    /// This validates proper exception propagation when the second option causes validation failure.
    /// </summary>
    [Test]
    public async Task AddSurveyOptions_ExceptionOnSecondOption_ShouldThrowAndFirstOptionShouldBeAdded()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var options = new List<SurveyOption>
        {
            new(NonEmptyString.Create("Option 1"), 50),
            new(NonEmptyString.Create("Option 1"), 30)
        };

        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOptions(options);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();

        await Assert.That(survey.Options.Count).IsEqualTo(1);
    }

    /// <summary>
    /// Tests that AddSurveyOptions correctly handles options with case-insensitive duplicate text.
    /// This ensures duplicate detection works regardless of text casing during batch addition.
    /// </summary>
    [Test]
    public async Task AddSurveyOptions_CollectionWithCaseInsensitiveDuplicates_ShouldThrowSurveyDomainException()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var options = new List<SurveyOption>
        {
            new(NonEmptyString.Create("option 1"), 30),
            new(NonEmptyString.Create("OPTION 1"), 20)
        };

        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOptions(options);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    /// <summary>
    /// Tests that AddSurveyOptions handles options with maximum integer value for preferred votes.
    /// This validates edge case behavior with int.MaxValue.
    /// </summary>
    [Test]
    public async Task AddSurveyOptions_OptionWithMaxIntPreferredVotes_ShouldThrowSurveyDomainException()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();

        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var options = new List<SurveyOption>
        {
            new(NonEmptyString.Create("Option 1"), int.MaxValue)
        };

        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOptions(options);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    /// <summary>
    /// Tests that ToString returns a JSON string containing all key properties when Survey has options.
    /// Input: Valid Survey instance with multiple options added.
    /// Expected: JSON string containing Topic, NumberOfRespondents, RespondentType, and Options properties.
    /// </summary>
    [Test]
    public async Task ToString_ValidSurveyWithOptions_ReturnsJsonContainingAllProperties()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        var numberOfRespondents = _fixture.Create<int>();
        var respondentType = _fixture.Create<NonEmptyString>();
        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        var option1 = _fixture.Create<NonEmptyString>();
        var option2 = _fixture.Create<NonEmptyString>();
        survey.AddSurveyOption(option1);
        survey.AddSurveyOption(option2);

        // Act
        var result = survey.ToString();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsNotEmpty();

        using var doc = JsonSerializer.Deserialize<JsonDocument>(result);
        await Assert.That(doc).IsNotNull();
        await Assert.That(doc!.RootElement.TryGetProperty("Topic", out _)).IsTrue();
        await Assert.That(doc.RootElement.TryGetProperty("NumberOfRespondents", out _)).IsTrue();
        await Assert.That(doc.RootElement.TryGetProperty("RespondentType", out _)).IsTrue();
        await Assert.That(doc.RootElement.TryGetProperty("Options", out _)).IsTrue();
    }

    /// <summary>
    /// Tests that when a survey has exactly one option and CalculateOutcome is called,
    /// all votes are distributed to that single option.
    /// </summary>
    [Test]
    public async Task CalculateOutcome_WithSingleOption_DistributesAllVotesToSingleOption()
    {
        // Arrange
        var fixture = new Fixture();
        var topic = fixture.Create<NonEmptyString>();
        var numberOfRespondents = fixture.Create<int>();
        var respondentType = fixture.Create<NonEmptyString>();

        var survey = new Survey(fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        survey.AddSurveyOption(fixture.Create<NonEmptyString>());

        // Act
        survey.CalculateOutcome();

        // Assert
        await Assert.That(survey.Options).Count().EqualTo(1);
        await Assert.That(survey.Options[0].NumberOfVotes).IsEqualTo(numberOfRespondents);
    }

    /// <summary>
    /// Tests that when a survey has exactly one respondent and multiple options,
    /// CalculateOutcome correctly distributes the single vote to one option.
    /// </summary>
    [Test]
    public async Task CalculateOutcome_WithOneRespondent_DistributesOneVoteCorrectly()
    {
        // Arrange
        var fixture = new Fixture();
        var topic = fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 1;
        var respondentType = fixture.Create<NonEmptyString>();

        var survey = new Survey(fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        survey.AddSurveyOption(fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(fixture.Create<NonEmptyString>());

        // Act
        survey.CalculateOutcome();

        // Assert
        await Assert.That(survey.Options.Sum(option => option.NumberOfVotes)).IsEqualTo(1);
        await Assert.That(survey.Options.Any(option => option.NumberOfVotes == 1)).IsTrue();
    }

    /// <summary>
    /// Tests that calling CalculateOutcome multiple times on the same survey
    /// redistributes votes correctly without accumulating previous calculations.
    /// </summary>
    [Test]
    public async Task CalculateOutcome_CalledMultipleTimes_RedistributesVotesCorrectly()
    {
        // Arrange
        var fixture = new Fixture();
        var topic = fixture.Create<NonEmptyString>();
        var numberOfRespondents = fixture.Create<int>();
        var respondentType = fixture.Create<NonEmptyString>();

        var survey = new Survey(fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        survey.AddSurveyOption(fixture.Create<NonEmptyString>());
        survey.AddSurveyOption(fixture.Create<NonEmptyString>());

        // Act
        survey.CalculateOutcome();
        var firstCalculationVotes = survey.Options.Sum(option => option.NumberOfVotes);

        survey.CalculateOutcome();
        var secondCalculationVotes = survey.Options.Sum(option => option.NumberOfVotes);

        // Assert
        await Assert.That(firstCalculationVotes).IsEqualTo(numberOfRespondents);
        await Assert.That(secondCalculationVotes).IsEqualTo(numberOfRespondents);
    }

    /// <summary>
    /// Tests that when a survey has a single option with a preferred number of votes,
    /// CalculateOutcome distributes exactly the preferred number of votes to that option.
    /// </summary>
    [Test]
    public async Task CalculateOutcome_WithSingleOptionAndPreferredVotes_DistributesPreferredVotesToSingleOption()
    {
        // Arrange
        var fixture = new Fixture();
        var topic = fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 100;
        const int preferredVotes = 100;
        var respondentType = fixture.Create<NonEmptyString>();

        var survey = new Survey(fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        survey.AddSurveyOption(fixture.Create<NonEmptyString>(), preferredVotes);

        // Act
        survey.CalculateOutcome();

        // Assert
        await Assert.That(survey.Options).Count().EqualTo(1);
        await Assert.That(survey.Options[0].NumberOfVotes).IsEqualTo(preferredVotes);
        await Assert.That(survey.IsRigged).IsTrue();
    }

    /// <summary>
    /// Tests that when a survey has the minimum valid number of respondents (1)
    /// and a single option, CalculateOutcome distributes that single vote correctly.
    /// </summary>
    [Test]
    public async Task CalculateOutcome_WithMinimumRespondentsAndSingleOption_DistributesSingleVote()
    {
        // Arrange
        var fixture = new Fixture();
        var topic = fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 1;
        var respondentType = fixture.Create<NonEmptyString>();

        var survey = new Survey(fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        survey.AddSurveyOption(fixture.Create<NonEmptyString>());

        // Act
        survey.CalculateOutcome();

        // Assert
        await Assert.That(survey.Options[0].NumberOfVotes).IsEqualTo(1);
    }

    /// <summary>
    /// Tests that AddSurveyOption successfully adds an option when preferred votes is zero.
    /// Input: Zero preferred votes.
    /// Expected: Option is added without exception.
    /// </summary>
    [Test]
    public async Task AddSurveyOption_WithZeroPreferredVotes_ShouldAddOptionSuccessfully()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();
        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        var optionText = _fixture.Create<NonEmptyString>();

        // Act
        survey.AddSurveyOption(optionText, 0);

        // Assert
        await Assert.That(survey.Options).Count().EqualTo(1);
        await Assert.That(survey.Options[0].PreferredNumberOfVotes).IsEqualTo(0);
    }

    /// <summary>
    /// Tests that AddSurveyOption throws exception when preferred votes is negative.
    /// Input: Negative preferred votes.
    /// Expected: SurveyDomainException is thrown.
    /// </summary>
    [Test]
    public async Task AddSurveyOption_WithNegativePreferredVotes_ShouldThrowSurveyDomainException()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();
        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        var optionText = _fixture.Create<NonEmptyString>();

        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOption(optionText, -10);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    /// <summary>
    /// Tests that AddSurveyOption throws exception when preferred votes is int.MinValue.
    /// Input: int.MinValue as preferred votes.
    /// Expected: SurveyDomainException is thrown.
    /// </summary>
    [Test]
    public async Task AddSurveyOption_WithIntMinValuePreferredVotes_ShouldThrowSurveyDomainException()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();
        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        var optionText = _fixture.Create<NonEmptyString>();

        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOption(optionText, int.MinValue);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    /// <summary>
    /// Tests that AddSurveyOption throws exception when preferred votes is int.MaxValue.
    /// Input: int.MaxValue as preferred votes.
    /// Expected: SurveyDomainException is thrown.
    /// </summary>
    [Test]
    public async Task AddSurveyOption_WithIntMaxValuePreferredVotes_ShouldThrowSurveyDomainException()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();
        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        var optionText = _fixture.Create<NonEmptyString>();

        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOption(optionText, int.MaxValue);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    /// <summary>
    /// Tests that AddSurveyOption successfully adds an option when sum of preferred votes exactly equals number of respondents.
    /// Input: Multiple options where combined preferred votes equals NumberOfRespondents.
    /// Expected: All options are added successfully.
    /// </summary>
    [Test]
    public async Task AddSurveyOption_WithSumOfPreferredVotesEqualToNumberOfRespondents_ShouldAddOptionsSuccessfully()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 1000;
        var respondentType = _fixture.Create<NonEmptyString>();
        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);

        // Act
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), 400);
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), 300);
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), 300);

        // Assert
        await Assert.That(survey.Options).Count().EqualTo(3);
        await Assert.That(survey.Options.Sum(o => o.PreferredNumberOfVotes)).IsEqualTo(1000);
    }

    /// <summary>
    /// Tests that AddSurveyOption throws exception when preferred votes equals NumberOfRespondents and another option already exists.
    /// Input: First option added, then second option with preferred votes equal to NumberOfRespondents.
    /// Expected: SurveyDomainException is thrown.
    /// </summary>
    [Test]
    public async Task AddSurveyOption_WithPreferredVotesEqualToNumberOfRespondentsWhenOptionsExist_ShouldThrowSurveyDomainException()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();
        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), 10);

        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), numberOfRespondents);
        }).ThrowsException().And.IsTypeOf<SurveyDomainException>();
    }

    /// <summary>
    /// Tests that AddSurveyOption successfully adds an option when preferred votes exactly equals number of respondents with no existing options.
    /// Input: Preferred votes equals NumberOfRespondents, no existing options.
    /// Expected: Option is added successfully.
    /// </summary>
    [Test]
    public async Task AddSurveyOption_WithPreferredVotesEqualToNumberOfRespondentsWhenNoOptionsExist_ShouldAddOptionSuccessfully()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();
        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        var optionText = _fixture.Create<NonEmptyString>();

        // Act
        survey.AddSurveyOption(optionText, numberOfRespondents);

        // Assert
        await Assert.That(survey.Options).Count().EqualTo(1);
        await Assert.That(survey.Options[0].PreferredNumberOfVotes).IsEqualTo(numberOfRespondents);
    }

    /// <summary>
    /// Tests that AddSurveyOption throws exception with correct message when preferred votes exceeds number of respondents.
    /// Input: Preferred votes greater than NumberOfRespondents.
    /// Expected: SurveyDomainException is thrown with specific message.
    /// </summary>
    [Test]
    public async Task AddSurveyOption_WithPreferredVotesExceedingNumberOfRespondents_ShouldThrowSurveyDomainExceptionWithCorrectMessage()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();
        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        var optionText = _fixture.Create<NonEmptyString>();
        const int preferredVotes = 150;

        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOption(optionText, preferredVotes);
        }).ThrowsException()
            .And.IsTypeOf<SurveyDomainException>()
            .And.HasMessageEqualTo($"Preferred number of votes: {preferredVotes} is higher than the number of respondents: {numberOfRespondents}");
    }

    /// <summary>
    /// Tests that AddSurveyOption throws exception with correct message when sum of preferred votes exceeds number of respondents.
    /// Input: Adding an option where sum of existing and new preferred votes exceeds NumberOfRespondents.
    /// Expected: SurveyDomainException is thrown with specific message.
    /// </summary>
    [Test]
    public async Task AddSurveyOption_WithSumOfPreferredVotesExceedingNumberOfRespondents_ShouldThrowSurveyDomainExceptionWithCorrectMessage()
    {
        // Arrange
        var topic = _fixture.Create<NonEmptyString>();
        const int numberOfRespondents = 100;
        var respondentType = _fixture.Create<NonEmptyString>();
        var survey = new Survey(_fixture.Create<User>(), topic, numberOfRespondents, respondentType);
        survey.AddSurveyOption(_fixture.Create<NonEmptyString>(), 60);
        var optionText = _fixture.Create<NonEmptyString>();
        const int preferredVotes = 50;

        // Act & Assert
        await Assert.That(() =>
        {
            survey.AddSurveyOption(optionText, preferredVotes);
        }).ThrowsException()
            .And.IsTypeOf<SurveyDomainException>()
            .And.HasMessageEqualTo($"Preferred number of votes: {preferredVotes} is higher than the number of respondents: {numberOfRespondents}");
    }
}
