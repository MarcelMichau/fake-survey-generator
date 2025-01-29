using System.Net;
using System.Net.Http.Json;
using Projects;
using TUnit.Assertions.AssertConditions.Throws;

namespace FakeSurveyGenerator.Acceptance.Tests;

public class SanityTests
{
    private const string UiProjectName = "fake-survey-generator-ui";
    private const string ApiProjectName = "fakesurveygeneratorapi";

    [Test]
    public async Task GivenRunningApp_WhenNavigatingToUiIndexPage_ThenResponseIsSuccessful()
    {
        Console.WriteLine("Running UI Index Page Test...");

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<FakeSurveyGenerator_Api>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient(UiProjectName);

        await Assert.That(async () =>
        {
            var response = await httpClient.GetAsync("/");
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        }).ThrowsNothing();
    }

    [Test]
    public async Task GivenRunningApp_WhenCallingApiHealthLiveEndpoint_ThenResponseIsSuccessful()
    {
        Console.WriteLine("Running API Health Live Endpoint Test...");

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<FakeSurveyGenerator_Api>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient(ApiProjectName);

        await Assert.That(async () =>
        {
            var response = await httpClient.GetAsync("health/live");
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        }).ThrowsNothing();
    }

    [Test]
    public async Task GivenRunningApp_WhenCallingApiHealthReadyEndpoint_ThenResponseIsSuccessful()
    {
        Console.WriteLine("Running API Health Ready Endpoint Test...");

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<FakeSurveyGenerator_Api>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient(ApiProjectName);

        await Assert.That(async () =>
        {
            var response = await httpClient.GetAsync("health/ready");
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        }).ThrowsNothing();
    }

    [Test]
    public async Task GivenRunningApp_WhenCallingApiVersionEndpoint_ThenValidVersionIsReturned()
    {
        Console.WriteLine("Running API Version Endpoint Test...");

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<FakeSurveyGenerator_Api>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient(ApiProjectName);

        await Assert.That(async () =>
        {
            var response = await httpClient.GetFromJsonAsync<VersionEndpointResponse>("api/admin/version");
            await Assert.That(response!.AssemblyVersion).IsNotNullOrWhitespace();
        }).ThrowsNothing();
    }

    private record VersionEndpointResponse(string AssemblyVersion);
}