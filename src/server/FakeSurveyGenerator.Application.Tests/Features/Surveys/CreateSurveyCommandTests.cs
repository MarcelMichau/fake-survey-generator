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
        var createSurveyCommand = new CreateSurveyCommand
        {
            SurveyTopic = _fixture.Create<string>(),
            NumberOfRespondents = _fixture.Create<int>(),
            RespondentType = _fixture.Create<string>(),
            SurveyOptions = new List<SurveyOptionDto> {
                new()
                {
                    OptionText = _fixture.Create<string>()
                },
                new()
                {
                    OptionText = _fixture.Create<string>()
                }
            }
        };

        var sut = new CreateSurveyCommandHandler(Context, _mockUserService);

        var result = await sut.Handle(createSurveyCommand, CancellationToken.None);

        var survey = result.Value;

        survey.Topic.Should().Be(createSurveyCommand.SurveyTopic);
        survey.NumberOfRespondents.Should().Be(createSurveyCommand.NumberOfRespondents);
        survey.RespondentType.Should().Be(createSurveyCommand.RespondentType);
    }

    [Fact]
    public async Task
        GivenCreateSurveyCommandHavingSurveyOptionsWithPreferredNumberOfVotes_WhenCallingHandle_ThenReturnedSurveyOptionsShouldHaveMatchingNumberOfVotes()
    {
        var createSurveyCommand = new CreateSurveyCommand
        {
            SurveyTopic = _fixture.Create<string>(),
            NumberOfRespondents = 500,
            RespondentType = _fixture.Create<string>(),
            SurveyOptions = new List<SurveyOptionDto> {
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
            }
        };

        var sut = new CreateSurveyCommandHandler(Context, _mockUserService);

        var result = await sut.Handle(createSurveyCommand, CancellationToken.None);

        var survey = result.Value;

        survey.Topic.Should().Be(createSurveyCommand.SurveyTopic);
        survey.NumberOfRespondents.Should().Be(createSurveyCommand.NumberOfRespondents);
        survey.RespondentType.Should().Be(createSurveyCommand.RespondentType);
        survey.IsRigged.Should().BeTrue();

        survey.Options.Should().HaveCount(2);

        survey.Options.Should().SatisfyRespectively(firstOption => { firstOption.NumberOfVotes.Should().Be(100); },
            secondOption => { secondOption.NumberOfVotes.Should().Be(400); });
    }
}