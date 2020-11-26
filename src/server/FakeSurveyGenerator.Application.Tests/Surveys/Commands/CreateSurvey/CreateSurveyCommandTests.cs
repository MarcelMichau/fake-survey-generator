using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Surveys.Commands.CreateSurvey
{
    public sealed class CreateSurveyCommandTests : CommandTestBase
    {
        [Fact]
        public async Task GivenValidCreateSurveyCommand_WhenCallingHandle_ThenNewSurveyShouldBeReturned()
        {
            var topic = "Tabs or spaces?";
            var numberOfRespondents = 1;
            var respondentType = "Developers";

            var options = new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = "Tabs"
                },
                new()
                {
                    OptionText = "Spaces"
                }
            };

            var createSurveyCommand = new CreateSurveyCommand(topic, numberOfRespondents, respondentType, options);

            var sut = new CreateSurveyCommandHandler(Context, Mapper, UserService);

            var result = await sut.Handle(createSurveyCommand, CancellationToken.None);

            var survey = result.Value;

            Assert.Equal(topic, survey.Topic);
            Assert.Equal(numberOfRespondents, survey.NumberOfRespondents);
            Assert.Equal(respondentType, survey.RespondentType);
        }

        [Fact]
        public async Task GivenCreateSurveyCommandHavingSurveyOptionsWithPreferredNumberOfVotes_WhenCallingHandle_ThenReturnedSurveyOptionsShouldHaveMatchingNumberOfVotes()
        {
            var topic = "Tabs or spaces?";
            var numberOfRespondents = 500;
            var respondentType = "Developers";

            var options = new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = "Tabs",
                    PreferredNumberOfVotes = 100
                },
                new()
                {
                    OptionText = "Spaces",
                    PreferredNumberOfVotes = 400
                }
            };

            var createSurveyCommand = new CreateSurveyCommand(topic, numberOfRespondents, respondentType, options);

            var sut = new CreateSurveyCommandHandler(Context, Mapper, UserService);

            var result = await sut.Handle(createSurveyCommand, CancellationToken.None);

            var survey = result.Value;

            Assert.Equal(topic, survey.Topic);
            Assert.Equal(numberOfRespondents, survey.NumberOfRespondents);
            Assert.Equal(respondentType, survey.RespondentType);
            Assert.Equal(100, survey.Options.First().NumberOfVotes);
            Assert.Equal(400, survey.Options.Last().NumberOfVotes);
        }
    }
}