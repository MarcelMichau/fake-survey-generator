using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace FakeSurveyGenerator.Acceptance.Tests;

public class SanityTests
{
    private const string UiProjectName = "fake-survey-generator-ui";
    private const string ApiProjectName = "fakesurveygeneratorapi";

    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(60);
    private readonly TimeSpan _defaultPollingInterval = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task GivenRunningApp_WhenNavigatingToUiIndexPage_ThenResponseIsSuccessful()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.FakeSurveyGenerator_Api>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient(UiProjectName);

        var act = async () =>
        {
            var response = await httpClient.GetAsync("/");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        };

        await act.Should().NotThrowAfterAsync(_defaultTimeout, _defaultPollingInterval);
    }

    [Fact]
    public async Task GivenRunningApp_WhenCallingApiHealthLiveEndpoint_ThenResponseIsSuccessful()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.FakeSurveyGenerator_Api>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient(ApiProjectName);

        // The app might take a while to create the database on startup, so we retry the request a few times
        var act = async () =>
        {
            var response = await httpClient.GetAsync("health/live");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        };

        await act.Should().NotThrowAfterAsync(_defaultTimeout, _defaultPollingInterval);
    }

    [Fact]
    public async Task GivenRunningApp_WhenCallingApiHealthReadyEndpoint_ThenResponseIsSuccessful()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.FakeSurveyGenerator_Api>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient(ApiProjectName);

        // The app might take a while to create the database on startup, so we retry the request a few times
        var act = async () =>
        {
            var response = await httpClient.GetAsync("health/ready");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        };

        await act.Should().NotThrowAfterAsync(_defaultTimeout, _defaultPollingInterval);
    }

    [Fact]
    public async Task GivenRunningApp_WhenCallingApiVersionEndpoint_ThenValidVersionIsReturned()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.FakeSurveyGenerator_Api>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient(ApiProjectName);

        // The app might take a while to create the database on startup, so we retry the request a few times
        var act = async () =>
        {
            var response = await httpClient.GetFromJsonAsync<VersionEndpointResponse>("api/admin/version");
            response!.AssemblyVersion.Should().NotBeNullOrWhiteSpace();
        };

        await act.Should().NotThrowAfterAsync(_defaultTimeout, _defaultPollingInterval);
    }

    

    private record VersionEndpointResponse(string AssemblyVersion);
}