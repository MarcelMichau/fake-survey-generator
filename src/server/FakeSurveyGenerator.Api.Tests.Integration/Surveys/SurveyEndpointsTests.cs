using System.Net;
using System.Net.Http.Json;
using AutoFixture;
using FakeSurveyGenerator.Api.Tests.Integration.Setup;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.TestHelpers;
using FluentAssertions;
using Xunit.Abstractions;

namespace FakeSurveyGenerator.Api.Tests.Integration.Surveys;

[Collection(nameof(IntegrationTestFixture))]
public sealed class SurveyEndpointsTests
{
    private readonly HttpClient _authenticatedClient;
    private readonly HttpClient _unauthenticatedClient;

    public SurveyEndpointsTests(IntegrationTestFixture testFixture, ITestOutputHelper testOutputHelper)
    {
        var fixture = new Fixture();

        _authenticatedClient = testFixture.Factory!
            .WithLoggerOutput(testOutputHelper)
            .WithSpecificUser(fixture.Create<TestUser>());

        _unauthenticatedClient = testFixture.Factory!
            .WithLoggerOutput(testOutputHelper)
            .CreateClient();
    }

    [Fact]
    public async Task
        GivenAuthenticatedClientWithValidCreateSurveyCommand_WhenCallingPostSurvey_ThenSuccessfulResponseWithNewlyCreatedSurveyShouldBeReturned()
    {
        var newUser = await RegisterNewUser();
        var newSurvey = await CreateSurvey();

        newSurvey.Options.Sum(option => option.NumberOfVotes).Should().Be(350);
        newSurvey.Topic.Should().Be("How awesome is this?");
        newSurvey.RespondentType.Should().Be("Individuals");
        newSurvey.Options.Should().AllSatisfy(option => { option.NumberOfVotes.Should().BeGreaterThan(0); });
        newSurvey.CreatedOn.Should().NotBe(DateTimeOffset.MinValue);
        newSurvey.CreatedBy.Should().Be(newUser.ExternalUserId);
    }

    [Fact]
    public async Task
        GivenUnauthenticatedClientWithValidCreateSurveyCommand_WhenCallingPostSurvey_ThenUnauthorizedResponseShouldBeReturned()
    {
        var createSurveyCommand = new CreateSurveyCommand("How unauthorized is this?", 400, "Unauthorized users",
            new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = "Very Unauthorized"
                },
                new()
                {
                    OptionText = "Completely Unauthorized"
                }
            });

        using var response = await _unauthenticatedClient.PostAsJsonAsync("/api/survey", createSurveyCommand);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task
        GivenInvalidCreateSurveyCommand_WhenCallingPostSurvey_ThenUnprocessableEntityResponseShouldBeReturned()
    {
        var createSurveyCommand = new CreateSurveyCommand("", 0, "",
            new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = ""
                }
            });

        using var response = await _authenticatedClient.PostAsJsonAsync("/api/survey", createSurveyCommand);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GivenExistingSurveyId_WhenCallingGetSurvey_ThenExistingSurveyShouldBeReturned()
    {
        var owner = await RegisterNewUser();
        var newSurvey = await CreateSurvey();

        var survey = await _authenticatedClient.GetFromJsonAsync<SurveyModel>($"api/survey/{newSurvey.Id}");

        survey!.Id.Should().Be(newSurvey.Id);
        survey.OwnerId.Should().Be(owner.Id);
        survey.Topic.Should().Be(newSurvey.Topic);
        survey.NumberOfRespondents.Should().Be(newSurvey.NumberOfRespondents);
        survey.RespondentType.Should().Be(newSurvey.RespondentType);
        survey.Options.First().OptionText.Should().Be(newSurvey.Options.First().OptionText);
    }

    [Fact]
    public async Task GivenExistingUserWithSurveys_WhenCallingGetUserSurveys_ThenExistingSurveysShouldBeReturned()
    {
        await RegisterNewUser();
        var newSurvey = await CreateSurvey();

        var surveys = await _authenticatedClient.GetFromJsonAsync<List<UserSurveyModel>>("api/survey/user");

        surveys!.Count.Should().BeGreaterThan(0);

        var createdSurvey = surveys.First(s => s.Id == newSurvey.Id);

        createdSurvey!.Id.Should().Be(newSurvey.Id);
        createdSurvey.Topic.Should().Be(newSurvey.Topic);
        createdSurvey.NumberOfRespondents.Should().Be(newSurvey.NumberOfRespondents);
        createdSurvey.RespondentType.Should().Be(newSurvey.RespondentType);
        createdSurvey.WinningOptionNumberOfVotes.Should().Be(newSurvey.Options.Max(o => o.NumberOfVotes));
    }

    [Fact]
    public async Task GivenNonExistentSurveyId_WhenCallingGetSurvey_ThenNotFoundResponseShouldBeReturned()
    {
        const int surveyId = 9000000;

        var response = await _authenticatedClient.GetAsync($"api/survey/{surveyId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<UserModel> RegisterNewUser()
    {
        var registerUserCommand = new RegisterUserCommand();

        var response = await _authenticatedClient.PostAsJsonAsync("/api/user/register", registerUserCommand);

        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<UserModel>();
        return user!;
    }

    private async Task<SurveyModel> CreateSurvey()
    {
        var createSurveyCommand = new CreateSurveyCommand("How awesome is this?", 350, "Individuals",
            new List<SurveyOptionDto>
            {
                new()
                {
                    OptionText = "Very awesome"
                },
                new()
                {
                    OptionText = "Not so much"
                }
            });

        var response = await _authenticatedClient.PostAsJsonAsync("/api/survey", createSurveyCommand);

        response.EnsureSuccessStatusCode();

        var survey = await response.Content.ReadFromJsonAsync<SurveyModel>();
        return survey!;
    }
}