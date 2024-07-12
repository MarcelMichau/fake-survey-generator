using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Projects;
using Xunit.Abstractions;

namespace FakeSurveyGenerator.Acceptance.Tests;

public class SanityTests(ITestOutputHelper output)
{
    private const string UiProjectName = "fake-survey-generator-ui";
    private const string ApiProjectName = "fakesurveygeneratorapi";
    private readonly TimeSpan _defaultPollingInterval = TimeSpan.FromSeconds(5);

    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(60);

    [Fact]
    public async Task GivenRunningApp_WhenNavigatingToUiIndexPage_ThenResponseIsSuccessful()
    {
        output.WriteLine("Running UI Index Page Test...");

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<FakeSurveyGenerator_Api>();
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
        output.WriteLine("Running API Health Live Endpoint Test...");

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<FakeSurveyGenerator_Api>();
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
        output.WriteLine("Running API Health Ready Endpoint Test...");

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<FakeSurveyGenerator_Api>();
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
        output.WriteLine("Running API Version Endpoint Test...");

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<FakeSurveyGenerator_Api>();
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