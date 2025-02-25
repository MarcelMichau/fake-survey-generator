using System.Net;
using System.Net.Http.Json;

namespace FakeSurveyGenerator.Acceptance.Tests;

public class SanityTests
{
    private const string UiProjectName = "fake-survey-generator-ui";
    private const string ApiProjectName = "fakesurveygeneratorapi";

    [ClassDataSource<AcceptanceTestFixture>(Shared = SharedType.PerTestSession)]
    public required AcceptanceTestFixture TestFixture { get; init; }

    private HttpClient ApiClient => TestFixture.App!.CreateHttpClient(ApiProjectName);
    private HttpClient UiClient => TestFixture.App!.CreateHttpClient(UiProjectName);

    [Test]
    public async Task GivenRunningApp_WhenNavigatingToUiIndexPage_ThenResponseIsSuccessful()
    {
        Console.WriteLine("Running UI Index Page Test...");

        var response = await UiClient.GetAsync("/");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenRunningApp_WhenCallingApiHealthLiveEndpoint_ThenResponseIsSuccessful()
    {
        Console.WriteLine("Running API Health Live Endpoint Test...");

        var response = await ApiClient.GetAsync("health/live");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenRunningApp_WhenCallingApiHealthReadyEndpoint_ThenResponseIsSuccessful()
    {
        Console.WriteLine("Running API Health Ready Endpoint Test...");

        var response = await ApiClient.GetAsync("health/ready");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenRunningApp_WhenCallingApiVersionEndpoint_ThenValidVersionIsReturned()
    {
        Console.WriteLine("Running API Version Endpoint Test...");

        var response = await ApiClient.GetFromJsonAsync<VersionEndpointResponse>("api/admin/version");
        await Assert.That(response!.AssemblyVersion).IsNotNullOrWhitespace();
    }

    private record VersionEndpointResponse(string AssemblyVersion);
}