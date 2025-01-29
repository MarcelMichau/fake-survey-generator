using System.Net;
using System.Net.Http.Json;
using AutoFixture;
using FakeSurveyGenerator.Api.Tests.Integration.Setup;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.TestHelpers;

namespace FakeSurveyGenerator.Api.Tests.Integration.Surveys;

public sealed class SurveyEndpointsTests
{
    [ClassDataSource<IntegrationTestFixture>(Shared = SharedType.PerTestSession)]
    public required IntegrationTestFixture TestFixture { get; init; }
    private readonly TestUser _testUser;

    private HttpClient AuthenticatedClient => TestFixture.Factory!
        .WithSpecificUser(_testUser);

    private HttpClient UnauthenticatedClient => TestFixture.Factory!
        .CreateClient();

    public SurveyEndpointsTests()
    {
        var fixture = new Fixture();
        _testUser = fixture.Create<TestUser>();
    }

    [Test]
    public async Task
        GivenAuthenticatedClientWithValidCreateSurveyCommand_WhenCallingPostSurvey_ThenSuccessfulResponseWithNewlyCreatedSurveyShouldBeReturned()
    {
        var newUser = await RegisterNewUser();
        var newSurvey = await CreateSurvey();

        await Assert.That(newSurvey.Options.Sum(option => option.NumberOfVotes)).IsEqualTo(350);
        await Assert.That(newSurvey.Topic).IsEqualTo("How awesome is this?");
        await Assert.That(newSurvey.RespondentType).IsEqualTo("Individuals");
        await Assert.That(newSurvey.Options.All(option => option.NumberOfVotes > 0)).IsTrue();
        await Assert.That(newSurvey.CreatedOn).IsNotEqualTo(default);
        await Assert.That(newSurvey.CreatedBy).IsEqualTo(newUser.ExternalUserId);
    }

    [Test]
    public async Task
        GivenUnauthenticatedClientWithValidCreateSurveyCommand_WhenCallingPostSurvey_ThenUnauthorizedResponseShouldBeReturned()
    {
        var createSurveyCommand = new CreateSurveyCommand
        {
            SurveyTopic = "How unauthorized is this?",
            NumberOfRespondents = 400,
            RespondentType = "Unauthorized users",
            SurveyOptions = new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = "Very Unauthorized"
                },
                new()
                {
                    OptionText = "Completely Unauthorized"
                }
            }
        };

        using var response = await UnauthenticatedClient.PostAsJsonAsync("/api/survey", createSurveyCommand);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task
        GivenInvalidCreateSurveyCommand_WhenCallingPostSurvey_ThenUnprocessableEntityResponseShouldBeReturned()
    {
        var createSurveyCommand = new CreateSurveyCommand
        {
            SurveyTopic = "",
            NumberOfRespondents = 0,
            RespondentType = "",
            SurveyOptions = new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = ""
                }
            }
        };

        using var response = await AuthenticatedClient.PostAsJsonAsync("/api/survey", createSurveyCommand);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task GivenExistingSurveyId_WhenCallingGetSurvey_ThenExistingSurveyShouldBeReturned()
    {
        var owner = await RegisterNewUser();
        var newSurvey = await CreateSurvey();

        var survey = await AuthenticatedClient.GetFromJsonAsync<SurveyModel>($"api/survey/{newSurvey.Id}");

        await Assert.That(survey!.Id).IsEqualTo(newSurvey.Id);
        await Assert.That(survey.OwnerId).IsEqualTo(owner.Id);
        await Assert.That(survey.Topic).IsEqualTo(newSurvey.Topic);
        await Assert.That(survey.NumberOfRespondents).IsEqualTo(newSurvey.NumberOfRespondents);
        await Assert.That(survey.RespondentType).IsEqualTo(newSurvey.RespondentType);

        await Assert.That(survey.Options.First().OptionText).IsEqualTo(newSurvey.Options.First().OptionText);
        await Assert.That(survey.Options.Last().OptionText).IsEqualTo(newSurvey.Options.Last().OptionText);
    }

    [Test]
    public async Task GivenExistingUserWithSurveys_WhenCallingGetUserSurveys_ThenExistingSurveysShouldBeReturned()
    {
        await RegisterNewUser();
        var newSurvey = await CreateSurvey();

        var surveys = await AuthenticatedClient.GetFromJsonAsync<List<UserSurveyModel>>("api/survey/user");

        await Assert.That(surveys!.Count).IsGreaterThan(0);

        var createdSurvey = surveys.First(s => s.Id == newSurvey.Id);

        await Assert.That(createdSurvey.Id).IsEqualTo(newSurvey.Id);
        await Assert.That(createdSurvey.Topic).IsEqualTo(newSurvey.Topic);
        await Assert.That(createdSurvey.NumberOfRespondents).IsEqualTo(newSurvey.NumberOfRespondents);
        await Assert.That(createdSurvey.RespondentType).IsEqualTo(newSurvey.RespondentType);
        await Assert.That(createdSurvey.WinningOptionNumberOfVotes).IsEqualTo(newSurvey.Options.Max(o => o.NumberOfVotes));
    }

    [Test]
    public async Task GivenNonExistentSurveyId_WhenCallingGetSurvey_ThenNotFoundResponseShouldBeReturned()
    {
        const int surveyId = 9000000;

        var response = await AuthenticatedClient.GetAsync($"api/survey/{surveyId}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    private async Task<UserModel> RegisterNewUser()
    {
        var registerUserCommand = new RegisterUserCommand();

        var response = await AuthenticatedClient.PostAsJsonAsync("/api/user/register", registerUserCommand);

        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<UserModel>();
        return user!;
    }

    private async Task<SurveyModel> CreateSurvey()
    {
        var createSurveyCommand = new CreateSurveyCommand
        {
            SurveyTopic = "How awesome is this?",
            NumberOfRespondents = 350,
            RespondentType = "Individuals",
            SurveyOptions = new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = "Very awesome"
                },
                new()
                {
                    OptionText = "Not so much"
                }
            }
        };

        var response = await AuthenticatedClient.PostAsJsonAsync("/api/survey", createSurveyCommand);

        response.EnsureSuccessStatusCode();

        var survey = await response.Content.ReadFromJsonAsync<SurveyModel>();
        return survey!;
    }
}
