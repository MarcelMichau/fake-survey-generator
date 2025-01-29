using AutoFixture;
using FakeSurveyGenerator.Application.Features.Surveys;

using FluentValidation.TestHelper;

namespace FakeSurveyGenerator.Application.Tests.Features.Surveys;

public sealed class CreateSurveyCommandValidatorTests
{
    private readonly IFixture _fixture = new Fixture();

    [Test]
    public async Task GivenValidCreateSurveyCommand_WhenValidatingCommand_ThenIsValidShouldBeTrue()
    {
        var command = new CreateSurveyCommand
        {
            SurveyTopic = _fixture.Create<string>(),
            NumberOfRespondents = 1,
            RespondentType = _fixture.Create<string>(),
            SurveyOptions = new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = _fixture.Create<string>(),
                    PreferredNumberOfVotes = _fixture.Create<int>()
                }
            }
        };

        var validator = new CreateSurveyCommandValidator();

        var result = validator.TestValidate(command);

        await Assert.That(result.IsValid).IsTrue();
    }

    [Test]
    public async Task GivenBlankSurveyTopic_WhenValidatingCommand_ThenIsValidShouldBeFalse()
    {
        var command = new CreateSurveyCommand
        {
            SurveyTopic = "",
            NumberOfRespondents = 1,
            RespondentType = _fixture.Create<string>(),
            SurveyOptions = new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = _fixture.Create<string>(),
                    PreferredNumberOfVotes = 1
                }
            }
        };

        var validator = new CreateSurveyCommandValidator();

        var result = validator.TestValidate(command);

        await Assert.That(result.IsValid).IsFalse();
        result.ShouldHaveValidationErrorFor(c => c.SurveyTopic);
    }

    [Test]
    public async Task GivenZeroNumberOfRespondents_WhenValidatingCommand_ThenIsValidShouldBeFalse()
    {
        var command = new CreateSurveyCommand
        {
            SurveyTopic = _fixture.Create<string>(),
            NumberOfRespondents = 0,
            RespondentType = _fixture.Create<string>(),
            SurveyOptions = new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = _fixture.Create<string>(),
                    PreferredNumberOfVotes = 1
                }
            }
        };

        var validator = new CreateSurveyCommandValidator();

        var result = validator.TestValidate(command);

        await Assert.That(result.IsValid).IsFalse();
        result.ShouldHaveValidationErrorFor(c => c.NumberOfRespondents);
    }

    [Test]
    public async Task GivenEmptyRespondentType_WhenValidatingCommand_ThenIsValidShouldBeFalse()
    {
        var command = new CreateSurveyCommand
        {
            SurveyTopic = _fixture.Create<string>(),
            NumberOfRespondents = 1,
            RespondentType = "",
            SurveyOptions = new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = _fixture.Create<string>(),
                    PreferredNumberOfVotes = 1
                }
            }
        };

        var validator = new CreateSurveyCommandValidator();

        var result = validator.TestValidate(command);

        await Assert.That(result.IsValid).IsFalse();
        result.ShouldHaveValidationErrorFor(c => c.RespondentType);
    }

    [Test]
    public async Task GivenEmptySurveyOptions_WhenValidatingCommand_ThenIsValidShouldBeFalse()
    {
        var command = new CreateSurveyCommand
        {
            SurveyTopic = _fixture.Create<string>(),
            NumberOfRespondents = 1,
            RespondentType = _fixture.Create<string>(),
            SurveyOptions = new List<SurveyOptionDto>()
        };

        var validator = new CreateSurveyCommandValidator();

        var result = validator.TestValidate(command);

        await Assert.That(result.IsValid).IsFalse();
        result.ShouldHaveValidationErrorFor(c => c.SurveyOptions);
    }

    [Test]
    public async Task GivenNullSurveyOptions_WhenValidatingCommand_ThenIsValidShouldBeFalse()
    {
        var command = new CreateSurveyCommand
        {
            SurveyTopic = _fixture.Create<string>(),
            NumberOfRespondents = 1,
            RespondentType = _fixture.Create<string>(),
            SurveyOptions = null!
        };

        var validator = new CreateSurveyCommandValidator();

        var result = validator.TestValidate(command);

        await Assert.That(result.IsValid).IsFalse();
        result.ShouldHaveValidationErrorFor(c => c.SurveyOptions);
    }

    [Test]
    public async Task GivenEmptySurveyOptionText_WhenValidatingCommand_ThenIsValidShouldBeFalse()
    {
        var command = new CreateSurveyCommand
        {
            SurveyTopic = _fixture.Create<string>(),
            NumberOfRespondents = 1,
            RespondentType = _fixture.Create<string>(),
            SurveyOptions = new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = ""
                }
            }
        };

        var validator = new CreateSurveyCommandValidator();

        var result = validator.TestValidate(command);

        await Assert.That(result.IsValid).IsFalse();
        result.ShouldHaveValidationErrorFor("SurveyOptions[0].OptionText");
    }
}
