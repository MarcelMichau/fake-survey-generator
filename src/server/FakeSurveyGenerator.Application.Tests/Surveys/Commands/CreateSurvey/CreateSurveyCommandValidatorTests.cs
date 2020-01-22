using System.Collections.Generic;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using Shouldly;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Surveys.Commands.CreateSurvey
{
    public class CreateSurveyCommandValidatorTests
    {
        [Fact]
        public void IsValid_ShouldBeTrue_WhenSurveyTopicIsSpecified()
        {
            var command = new CreateSurveyCommand("Test", 1, "Test", new List<SurveyOptionDto>());

            var validator = new CreateSurveyCommandValidator();

            var result = validator.Validate(command);

            result.IsValid.ShouldBe(true);
        }
    }
}
