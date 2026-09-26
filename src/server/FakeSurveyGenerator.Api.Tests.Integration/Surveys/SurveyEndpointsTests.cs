using System.Net;
using AutoFixture;
using FakeSurveyGenerator.Api.Tests.Integration.Setup;
using FakeSurveyGenerator.Application.Features.Notifications;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.Shared.Notifications;
using FakeSurveyGenerator.Application.TestHelpers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;

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
    public async Task GivenSurveyCreatedThroughApi_WhenSaved_ThenSaveChangesInterceptorPublishesNotification()
    {
        var notificationService = Substitute.For<INotificationService>();
        using var notificationFactory = TestFixture.Factory!.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<INotificationService>();
                services.AddSingleton(notificationService);
            }));
        using var client = notificationFactory.WithSpecificUser(_testUser);

        using var registrationResponse = await client.PostAsJsonAsync("/api/user/register", new RegisterUserCommand());
        registrationResponse.EnsureSuccessStatusCode();

        var createSurveyCommand = new CreateSurveyCommand
        {
            SurveyTopic = "Domain event integration test",
            NumberOfRespondents = 1,
            RespondentType = "Test respondents",
            SurveyOptions = [new SurveyOptionDto { OptionText = "Test option" }]
        };
        using var surveyResponse = await client.PostAsJsonAsync("/api/survey", createSurveyCommand);
        surveyResponse.EnsureSuccessStatusCode();

        await notificationService.Received(1).SendMessage(
            Arg.Is<MessageModel>(message =>
                message.From == "System" &&
                message.To == "Whom It May Concern" &&
                message.Subject == "New Survey Created" &&
                message.Body.StartsWith("Survey with ID:", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task
        GivenAuthenticatedClientWithValidSurvey_WhenCallingPostAnalyzeSurvey_ThenAnalysisShouldBeReturned()
    {
        var command = new AnalyzeSurveyCommand
        {
            SurveyTopic = "Do you prefer tabs or spaces?",
            RespondentType = "Developers",
            SurveyOptions =
            [
                new SurveyOptionDto { OptionText = "Tabs" },
                new SurveyOptionDto { OptionText = "Spaces" }
            ]
        };

        using var response = await AuthenticatedClient.PostAsJsonAsync("/api/survey/analyze", command);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var analysis = await response.Content.ReadFromJsonAsync<SurveyAnalysisModel>();
        await Assert.That(analysis).IsNotNull();
        await Assert.That(analysis!.ResponseShape).IsEqualTo("single_choice");
        await Assert.That(analysis.Warnings).IsEmpty();
    }

    [Test]
    public async Task GivenInvalidAnalyzeSurveyCommand_WhenCallingAnalyzeSurvey_ThenUnprocessableEntityIsReturned()
    {
        var command = new AnalyzeSurveyCommand
        {
            SurveyTopic = "",
            RespondentType = "",
            SurveyOptions = []
        };

        using var response = await AuthenticatedClient.PostAsJsonAsync("/api/survey/analyze", command);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task GivenTypeSafeApiKeyIsMissing_WhenCallingAnalyzeSurvey_ThenServiceUnavailableIsReturned()
    {
        await using var factory = TestFixture.Factory!.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TYPESAFE_API_KEY"] = ""
                }));
            builder.ConfigureTestServices(services =>
            {
                var testAnalyzer = services.Last(service => service.ServiceType == typeof(ISurveySemanticAnalyzer));
                services.Remove(testAnalyzer);
            });
        });
        using var client = factory.WithSpecificUser(_testUser);
        var command = new AnalyzeSurveyCommand
        {
            SurveyTopic = "Do you prefer tabs or spaces?",
            RespondentType = "Developers",
            SurveyOptions =
            [
                new SurveyOptionDto { OptionText = "Tabs" },
                new SurveyOptionDto { OptionText = "Spaces" }
            ]
        };

        using var response = await client.PostAsJsonAsync("/api/survey/analyze", command);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.ServiceUnavailable);
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
            SurveyOptions =
            [
                new SurveyOptionDto
                {
                    OptionText = "Very Unauthorized"
                },
                new SurveyOptionDto
                {
                    OptionText = "Completely Unauthorized"
                }
            ]
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
            SurveyOptions =
            [
                new SurveyOptionDto
                {
                    OptionText = ""
                }
            ]
        };

        using var response = await AuthenticatedClient.PostAsJsonAsync("/api/survey", createSurveyCommand);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task
        GivenInvalidCreateSurveyCommand_WhenCallingPostSurvey_ThenValidationErrorsShouldBeLogged()
    {
        var logIndexBefore = TestLogSink.Shared.Entries.Count;

        var createSurveyCommand = new CreateSurveyCommand
        {
            SurveyTopic = "",
            NumberOfRespondents = 0,
            RespondentType = "",
            SurveyOptions =
            [
                new SurveyOptionDto
                {
                    OptionText = ""
                }
            ]
        };

        using var response = await AuthenticatedClient.PostAsJsonAsync("/api/survey", createSurveyCommand);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.UnprocessableEntity);

        var validationLog = TestLogSink.Shared.Entries.Skip(logIndexBefore).LastOrDefault(entry =>
            entry.Level == LogLevel.Warning
            && entry.EventId.Id == 1
            && entry.Category == "FakeSurveyGenerator.Api.Filters.ValidationLoggingEndpointFilter");

        await Assert.That(validationLog).IsNotNull();

        await Assert.That(validationLog!.Message.Contains("Validation failure on Endpoint: CreateSurvey")).IsTrue();
        await Assert.That(validationLog.Message.Contains("User:")).IsTrue();
        await Assert.That(validationLog.Message.Contains("Unknown Identity")).IsFalse();

        var state = validationLog.State;
        await Assert.That(state).IsNotNull();
        await Assert.That(HasStateKey(state, "Error.SurveyTopic")).IsTrue();
        await Assert.That(HasStateKey(state, "Error.SurveyOptions[0].OptionText")).IsTrue();

        var surveyTopicErrors = GetStateStringArray(state, "Error.SurveyTopic");
        var optionTextErrors = GetStateStringArray(state, "Error.SurveyOptions[0].OptionText");

        await Assert.That(surveyTopicErrors).IsNotNull();
        await Assert.That(optionTextErrors).IsNotNull();
        await Assert.That(surveyTopicErrors!.Length).IsGreaterThan(0);
        await Assert.That(optionTextErrors!.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task
        GivenInvalidCreateSurveyCommand_WhenCallingPostSurvey_ThenRequestShouldBeLogged()
    {
        var logIndexBefore = TestLogSink.Shared.Entries.Count;

        var createSurveyCommand = new CreateSurveyCommand
        {
            SurveyTopic = "",
            NumberOfRespondents = 0,
            RespondentType = "",
            SurveyOptions =
            [
                new SurveyOptionDto
                {
                    OptionText = ""
                }
            ]
        };

        using var response = await AuthenticatedClient.PostAsJsonAsync("/api/survey", createSurveyCommand);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.UnprocessableEntity);

        var hasRequestLog = TestLogSink.Shared.Entries.Skip(logIndexBefore).Any(entry =>
            entry.Category == "FakeSurveyGenerator.Api.Filters.RequestLoggingEndpointFilter"
            && entry.Message.Contains("Request to Endpoint: CreateSurvey")
            && entry.Message.Contains("User:")
            && !entry.Message.Contains("Unknown Identity"));

        await Assert.That(hasRequestLog).IsTrue();
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
        await Assert.That(createdSurvey.WinningOptionNumberOfVotes)
            .IsEqualTo(newSurvey.Options.Max(o => o.NumberOfVotes));
    }

    [Test]
    public async Task
        GivenCreateSurveyCommandWithDuplicateOptions_WhenCallingPostSurvey_ThenUnprocessableEntityResponseShouldBeReturned()
    {
        await RegisterNewUser();
        var createSurveyCommand = new CreateSurveyCommand
        {
            SurveyTopic = "Duplicate test",
            NumberOfRespondents = 100,
            RespondentType = "Testers",
            SurveyOptions =
            [
                new SurveyOptionDto { OptionText = "Option A" },
                new SurveyOptionDto { OptionText = "Option A" }
            ]
        };

        using var response = await AuthenticatedClient.PostAsJsonAsync("/api/survey", createSurveyCommand);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.UnprocessableEntity);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>();
        await Assert.That(body!.Values.SelectMany(v => v)).Contains("Duplicate survey option.");
    }

    [Test]
    public async Task GivenNonExistentSurveyId_WhenCallingGetSurvey_ThenNotFoundResponseShouldBeReturned()
    {
        const int surveyId = 9000000;

        var response = await AuthenticatedClient.GetAsync($"api/survey/{surveyId}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task
        GivenOwnerDeletingOwnSurvey_WhenCallingDeleteSurvey_ThenNoContentResponseShouldBeReturnedAndSurveyShouldNoLongerExist()
    {
        await RegisterNewUser();
        var newSurvey = await CreateSurvey();

        using var deleteResponse = await AuthenticatedClient.DeleteAsync($"api/survey/{newSurvey.Id}");

        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        using var getResponse = await AuthenticatedClient.GetAsync($"api/survey/{newSurvey.Id}");

        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task
        GivenNonOwnerAttemptingDelete_WhenCallingDeleteSurvey_ThenForbiddenResponseShouldBeReturnedAndSurveyShouldStillExist()
    {
        await RegisterNewUser();
        var newSurvey = await CreateSurvey();

        var otherUser = new Fixture().Create<TestUser>();
        var otherClient = TestFixture.Factory!.WithSpecificUser(otherUser);

        var registerOtherUserResponse =
            await otherClient.PostAsJsonAsync("/api/user/register", new RegisterUserCommand());
        registerOtherUserResponse.EnsureSuccessStatusCode();

        using var deleteResponse = await otherClient.DeleteAsync($"api/survey/{newSurvey.Id}");

        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        using var getResponse = await AuthenticatedClient.GetAsync($"api/survey/{newSurvey.Id}");

        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task
        GivenUnauthenticatedClient_WhenCallingDeleteSurvey_ThenUnauthorizedResponseShouldBeReturned()
    {
        using var response = await UnauthenticatedClient.DeleteAsync("/api/survey/1");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GivenNonExistentSurveyId_WhenCallingDeleteSurvey_ThenNotFoundResponseShouldBeReturned()
    {
        await RegisterNewUser();
        const int surveyId = 9000001;

        using var response = await AuthenticatedClient.DeleteAsync($"api/survey/{surveyId}");

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
            SurveyOptions =
            [
                new SurveyOptionDto
                {
                    OptionText = "Very awesome"
                },
                new SurveyOptionDto
                {
                    OptionText = "Not so much"
                }
            ]
        };

        var response = await AuthenticatedClient.PostAsJsonAsync("/api/survey", createSurveyCommand);

        response.EnsureSuccessStatusCode();

        var survey = await response.Content.ReadFromJsonAsync<SurveyModel>();
        return survey!;
    }

    private static bool HasStateKey(
        IReadOnlyList<KeyValuePair<string, object?>> state,
        string key)
    {
        return state.Any(item => item.Key == key);
    }

    private static string[]? GetStateStringArray(
        IReadOnlyList<KeyValuePair<string, object?>> state,
        string key)
    {
        return state.FirstOrDefault(item => item.Key == key).Value as string[];
    }
}
