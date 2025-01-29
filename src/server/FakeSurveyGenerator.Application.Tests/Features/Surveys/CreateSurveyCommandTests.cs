using AutoFixture;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Shared.Identity;
using FakeSurveyGenerator.Application.TestHelpers;
using FakeSurveyGenerator.Application.Tests.Setup;
using NSubstitute;

namespace FakeSurveyGenerator.Application.Tests.Features.Surveys;

public sealed class CreateSurveyCommandTests
{
    [ClassDataSource<TestFixture>]
    public required TestFixture Fixture { get; init; }

    private readonly Fixture _fixture = new();
    private readonly IUserService _mockUserService = Substitute.For<IUserService>();

    public CreateSurveyCommandTests()
    {
        _mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(TestUser.Instance);
    }

    [Test]
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

        var sut = new CreateSurveyCommandHandler(Fixture.Context, _mockUserService);

        var result = await sut.Handle(createSurveyCommand, CancellationToken.None);

        var survey = result.Value;

        await Assert.That(survey.Topic).IsEqualTo(createSurveyCommand.SurveyTopic);
        await Assert.That(survey.NumberOfRespondents).IsEqualTo(createSurveyCommand.NumberOfRespondents);
        await Assert.That(survey.RespondentType).IsEqualTo(createSurveyCommand.RespondentType);
    }

    [Test]
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

        var sut = new CreateSurveyCommandHandler(Fixture.Context, _mockUserService);

        var result = await sut.Handle(createSurveyCommand, CancellationToken.None);

        var survey = result.Value;

        await Assert.That(survey.Topic).IsEqualTo(createSurveyCommand.SurveyTopic);
        await Assert.That(survey.NumberOfRespondents).IsEqualTo(createSurveyCommand.NumberOfRespondents);
        await Assert.That(survey.RespondentType).IsEqualTo(createSurveyCommand.RespondentType);

        await Assert.That(survey.IsRigged).IsTrue();

        await Assert.That(survey.Options.Count).IsEqualTo(2);

        await Assert.That(survey.Options[0].NumberOfVotes).IsEqualTo(100);
        await Assert.That(survey.Options[1].NumberOfVotes).IsEqualTo(400);
    }
}