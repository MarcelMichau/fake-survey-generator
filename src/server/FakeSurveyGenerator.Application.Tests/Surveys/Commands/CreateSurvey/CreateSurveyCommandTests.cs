using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using FakeSurveyGenerator.Data;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Surveys.Commands.CreateSurvey;

public sealed class CreateSurveyCommandTests : CommandTestBase
{
    private readonly Fixture _fixture = new();
    private readonly IUserService _mockUserService = Substitute.For<IUserService>();

    public CreateSurveyCommandTests()
    {
        _mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(new TestUser());
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

        var sut = new CreateSurveyCommandHandler(Context, Mapper, _mockUserService);

        var result = await sut.Handle(createSurveyCommand, CancellationToken.None);

        var survey = result.Value;

        survey.Topic.Should().Be(topic);
        survey.NumberOfRespondents.Should().Be(numberOfRespondents);
        survey.RespondentType.Should().Be(respondentType);
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

        var sut = new CreateSurveyCommandHandler(Context, Mapper, _mockUserService);

        var result = await sut.Handle(createSurveyCommand, CancellationToken.None);

        var survey = result.Value;

        survey.Topic.Should().Be(topic);
        survey.NumberOfRespondents.Should().Be(numberOfRespondents);
        survey.RespondentType.Should().Be(respondentType);

        survey.Options.Should().HaveCount(2);
        survey.Options.First().NumberOfVotes.Should().Be(100);
        survey.Options.Last().NumberOfVotes.Should().Be(400);
    }
}