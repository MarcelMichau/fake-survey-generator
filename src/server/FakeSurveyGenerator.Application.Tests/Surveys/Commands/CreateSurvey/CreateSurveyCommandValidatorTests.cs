using System.Collections.Generic;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using Shouldly;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Surveys.Commands.CreateSurvey
{
    public class CreateSurveyCommandValidatorTests
    {
        [Fact]
        public void IsValid_ShouldBeTrue_WhenValidValuesAreSpecified()
        {
            var command = new CreateSurveyCommand("Test", 1, "Test", new List<SurveyOptionDto>
            {
                new SurveyOptionDto
                {
                    OptionText = "Test",
                    PreferredNumberOfVotes = 1
                }
            });

            var validator = new CreateSurveyCommandValidator();

            var result = validator.Validate(command);

            result.IsValid.ShouldBe(true);
        }

        [Fact]
        public void IsValid_ShouldBeFalse_WhenSurveyTopicIsBlank()
        {
            var command = new CreateSurveyCommand("", 1, "Test", new List<SurveyOptionDto>());

            var validator = new CreateSurveyCommandValidator();

            var result = validator.Validate(command);

            result.IsValid.ShouldBe(false);
        }

        [Fact]
        public void IsValid_ShouldBeFalse_WhenNumberOfRespondentsIsZero()
        {
            var command = new CreateSurveyCommand("Test", 0, "Test", new List<SurveyOptionDto>());

            var validator = new CreateSurveyCommandValidator();

            var result = validator.Validate(command);

            result.IsValid.ShouldBe(false);
        }

        [Fact]
        public void IsValid_ShouldBeFalse_WhenRespondentTypeIsNotSpecified()
        {
            var command = new CreateSurveyCommand("Test", 1, "", new List<SurveyOptionDto>());

            var validator = new CreateSurveyCommandValidator();

            var result = validator.Validate(command);

            result.IsValid.ShouldBe(false);
        }

        [Fact]
        public void IsValid_ShouldBeFalse_WhenSurveyOptionsAreEmpty()
        {
            var command = new CreateSurveyCommand("Test", 1, "Test", new List<SurveyOptionDto>());

            var validator = new CreateSurveyCommandValidator();

            var result = validator.Validate(command);

            result.IsValid.ShouldBe(false);
        }
    }
}
