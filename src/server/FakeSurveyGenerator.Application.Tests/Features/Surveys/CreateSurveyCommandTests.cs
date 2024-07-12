using AutoFixture;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Shared.Identity;
using FakeSurveyGenerator.Application.TestHelpers;
using FakeSurveyGenerator.Application.Tests.Setup;
using FluentAssertions;
using NSubstitute;

namespace FakeSurveyGenerator.Application.Tests.Features.Surveys;

public sealed class CreateSurveyCommandTests : CommandTestBase
{
    private readonly Fixture _fixture = new();
    private readonly IUserService _mockUserService = Substitute.For<IUserService>();

    public CreateSurveyCommandTests()
    {
        _mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(TestUser.Instance);
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

        var sut = new CreateSurveyCommandHandler(Context, _mockUserService);

        var result = await sut.Handle(createSurveyCommand, CancellationToken.None);

        var survey = result.Value;

        survey.Topic.Should().Be(topic);
        survey.NumberOfRespondents.Should().Be(numberOfRespondents);
        survey.RespondentType.Should().Be(respondentType);
    }

    [Fact]
    public async Task
        GivenCreateSurveyCommandHavingSurveyOptionsWithPreferredNumberOfVotes_WhenCallingHandle_ThenReturnedSurveyOptionsShouldHaveMatchingNumberOfVotes()
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

        var sut = new CreateSurveyCommandHandler(Context, _mockUserService);

        var result = await sut.Handle(createSurveyCommand, CancellationToken.None);

        var survey = result.Value;

        survey.Topic.Should().Be(topic);
        survey.NumberOfRespondents.Should().Be(numberOfRespondents);
        survey.RespondentType.Should().Be(respondentType);
        survey.IsRigged.Should().BeTrue();

        survey.Options.Should().HaveCount(2);

        survey.Options.Should().SatisfyRespectively(firstOption => { firstOption.NumberOfVotes.Should().Be(100); },
            secondOption => { secondOption.NumberOfVotes.Should().Be(300); });
    }
}