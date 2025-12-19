using System.Net;
using System.Net.Http.Json;

namespace FakeSurveyGenerator.Acceptance.Tests;

public class SanityTests
{
    [ClassDataSource<AcceptanceTestFixture>(Shared = SharedType.PerTestSession)]
    public required AcceptanceTestFixture TestFixture { get; init; }

    private HttpClient ApiClient => TestFixture.App!.CreateHttpClient("api");
    private HttpClient UiClient => TestFixture.App!.CreateHttpClient("ui", "https");

    [Test]
    public async Task GivenRunningApp_WhenNavigatingToUiIndexPage_ThenResponseIsSuccessful(CancellationToken cancellationToken)
    {
        Console.WriteLine("Running UI Index Page Test...");

        var response = await UiClient.GetAsync("/", cancellationToken);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenRunningApp_WhenCallingApiHealthLiveEndpoint_ThenResponseIsSuccessful(CancellationToken cancellationToken)
    {
        Console.WriteLine("Running API Health Live Endpoint Test...");

        var response = await ApiClient.GetAsync("health/live", cancellationToken);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenRunningApp_WhenCallingApiHealthReadyEndpoint_ThenResponseIsSuccessful(CancellationToken cancellationToken)
    {
        Console.WriteLine("Running API Health Ready Endpoint Test...");

        var response = await ApiClient.GetAsync("health/ready", cancellationToken);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenRunningApp_WhenCallingApiVersionEndpoint_ThenValidVersionIsReturned(CancellationToken cancellationToken)
    {
        Console.WriteLine("Running API Version Endpoint Test...");

        var response = await ApiClient.GetFromJsonAsync<VersionEndpointResponse>("api/admin/version", cancellationToken: cancellationToken);
        await Assert.That(response!.AssemblyVersion).IsNotNullOrWhiteSpace();
    }

    private record VersionEndpointResponse(string AssemblyVersion);
}