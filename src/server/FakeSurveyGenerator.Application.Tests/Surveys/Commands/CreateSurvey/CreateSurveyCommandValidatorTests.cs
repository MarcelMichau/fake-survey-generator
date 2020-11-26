using System.Collections.Generic;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using FluentAssertions;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Surveys.Commands.CreateSurvey
{
    public sealed class CreateSurveyCommandValidatorTests
    {
        [Fact]
        public void GivenValidCreateSurveyCommand_WhenValidatingCommand_ThenIsValidShouldBeTrue()
        {
            var command = new CreateSurveyCommand("Test", 1, "Test", new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = "Test",
                    PreferredNumberOfVotes = 1
                }
            });

            var validator = new CreateSurveyCommandValidator();

            var result = validator.Validate(command);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void GivenBlankSurveyTopic_WhenValidatingCommand_ThenIsValidShouldBeFalse()
        {
            var command = new CreateSurveyCommand("", 1, "Test", new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = "Test",
                    PreferredNumberOfVotes = 1
                }
            });

            var validator = new CreateSurveyCommandValidator();

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateSurveyCommand.SurveyTopic));
        }

        [Fact]
        public void GivenZeroNumberOfRespondents_WhenValidatingCommand_ThenIsValidShouldBeFalse()
        {
            var command = new CreateSurveyCommand("Test", 0, "Test", new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = "Test",
                    PreferredNumberOfVotes = 1
                }
            });

            var validator = new CreateSurveyCommandValidator();

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateSurveyCommand.NumberOfRespondents));
        }

        [Fact]
        public void GivenEmptyRespondentType_WhenValidatingCommand_ThenIsValidShouldBeFalse()
        {
            var command = new CreateSurveyCommand("Test", 1, "", new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = "Test",
                    PreferredNumberOfVotes = 1
                }
            });

            var validator = new CreateSurveyCommandValidator();

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateSurveyCommand.RespondentType));
        }

        [Fact]
        public void GivenEmptySurveyOptions_WhenValidatingCommand_ThenIsValidShouldBeFalse()
        {
            var command = new CreateSurveyCommand("Test", 1, "Test", new List<SurveyOptionDto>());

            var validator = new CreateSurveyCommandValidator();

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateSurveyCommand.SurveyOptions));
        }

        [Fact]
        public void GivenNullSurveyOptions_WhenValidatingCommand_ThenIsValidShouldBeFalse()
        {
            var command = new CreateSurveyCommand("Test", 1, "Test", null);

            var validator = new CreateSurveyCommandValidator();

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateSurveyCommand.SurveyOptions));
        }

        [Fact]
        public void GivenEmptySurveyOptionText_WhenValidatingCommand_ThenIsValidShouldBeFalse()
        {
            var command = new CreateSurveyCommand("Test", 1, "Test", new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = ""
                }
            });

            var validator = new CreateSurveyCommandValidator();

            var result = validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.PropertyName == "SurveyOptions[0].OptionText");
        }
    }
}
