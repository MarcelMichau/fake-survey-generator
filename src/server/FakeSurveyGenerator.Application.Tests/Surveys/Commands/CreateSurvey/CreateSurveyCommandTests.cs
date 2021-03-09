using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using FakeSurveyGenerator.Data;
using Moq;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Surveys.Commands.CreateSurvey
{
    public sealed class CreateSurveyCommandTests : CommandTestBase
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IUserService> _mockUserService = new();

        public CreateSurveyCommandTests()
        {
            _mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TestUser());
        }

        [Fact]
        public async Task GivenValidCreateSurveyCommand_WhenCallingHandle_ThenNewSurveyShouldBeReturned()
        {
            var topic = _fixture.Create<string>();
            var numberOfRespondents = _fixture.Create<int>();
            var respondentType = _fixture.Create<string>();

            var options = new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = _fixture.Create<string>()
                },
                new()
                {
                    OptionText = _fixture.Create<string>()
                }
            };

            var createSurveyCommand = new CreateSurveyCommand(topic, numberOfRespondents, respondentType, options);

            var sut = new CreateSurveyCommandHandler(Context, Mapper, _mockUserService.Object);

            var result = await sut.Handle(createSurveyCommand, CancellationToken.None);

            var survey = result.Value;

            Assert.Equal(topic, survey.Topic);
            Assert.Equal(numberOfRespondents, survey.NumberOfRespondents);
            Assert.Equal(respondentType, survey.RespondentType);
        }

        [Fact]
        public async Task GivenCreateSurveyCommandHavingSurveyOptionsWithPreferredNumberOfVotes_WhenCallingHandle_ThenReturnedSurveyOptionsShouldHaveMatchingNumberOfVotes()
        {
            var topic = _fixture.Create<string>();
            const int numberOfRespondents = 500;
            var respondentType = _fixture.Create<string>();

            var options = new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = _fixture.Create<string>(),
                    PreferredNumberOfVotes = 100
                },
                new()
                {
                    OptionText = _fixture.Create<string>(),
                    PreferredNumberOfVotes = 400
                }
            };

            var createSurveyCommand = new CreateSurveyCommand(topic, numberOfRespondents, respondentType, options);

            var sut = new CreateSurveyCommandHandler(Context, Mapper, _mockUserService.Object);

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