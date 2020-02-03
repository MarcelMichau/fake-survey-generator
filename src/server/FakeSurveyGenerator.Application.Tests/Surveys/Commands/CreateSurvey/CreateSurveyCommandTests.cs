using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Surveys.Commands.CreateSurvey
{
    public class CreateSurveyCommandTests : CommandTestBase
    {
        [Fact]
        public async Task Handle_GivenValidRequest_ShouldRaiseSurveyCreatedNotification()
        {
            var topic = "Tabs or spaces?";
            var numberOfRespondents = 1;
            var respondentType = "Developers";

            var options = new List<SurveyOptionDto>
            {
                new SurveyOptionDto
                {
                    OptionText = "Tabs"
                },
                new SurveyOptionDto
                {
                    OptionText = "Spaces"
                }
            };

            var createSurveyCommand = new CreateSurveyCommand(topic, numberOfRespondents, respondentType, options);

            var sut = new CreateSurveyCommandHandler(Context, Mapper);

            var result = await sut.Handle(createSurveyCommand, CancellationToken.None);

            Assert.Equal(topic, result.Topic);
            Assert.Equal(numberOfRespondents, result.NumberOfRespondents);
            Assert.Equal(respondentType, result.RespondentType);
            Assert.True(result.CreatedOn < DateTime.UtcNow, "The createdOn date was not in the past");
        }

        [Fact]
        public async Task Handle_GivenValidPreferredNumberOfVotesRequest_ShouldHaveCorrectVoteDistribution()
        {
            var topic = "Tabs or spaces?";
            var numberOfRespondents = 500;
            var respondentType = "Developers";

            var options = new List<SurveyOptionDto>
            {
                new SurveyOptionDto
                {
                    OptionText = "Tabs",
                    PreferredNumberOfVotes = 100
                },
                new SurveyOptionDto
                {
                    OptionText = "Spaces",
                    PreferredNumberOfVotes = 400
                }
            };

            var createSurveyCommand = new CreateSurveyCommand(topic, numberOfRespondents, respondentType, options);

            var sut = new CreateSurveyCommandHandler(Context, Mapper);

            var result = await sut.Handle(createSurveyCommand, CancellationToken.None);

            Assert.Equal(topic, result.Topic);
            Assert.Equal(numberOfRespondents, result.NumberOfRespondents);
            Assert.Equal(respondentType, result.RespondentType);
            Assert.True(result.CreatedOn < DateTime.UtcNow, "The createdOn date was not in the past");
            Assert.Equal(100, result.Options.First().NumberOfVotes);
            Assert.Equal(400, result.Options.Last().NumberOfVotes);
        }
    }
}
