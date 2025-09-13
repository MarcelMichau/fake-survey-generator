using FakeSurveyGenerator.Api.Tests.Integration.Setup;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

public sealed class OpenApiTests
{
    [ClassDataSource<IntegrationTestFixture>(Shared = SharedType.PerTestSession)]
    public required IntegrationTestFixture TestFixture { get; init; }

    private HttpClient Client => TestFixture.Factory!.CreateClient();

    [Test]
    public async Task GivenAnyUser_WhenMakingRequestToApiDocsRoute_ThenSuccessResponseShouldBeReturned()
    {
        var response = await Client.GetAsync("/api-docs");
        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task GivenAnyUser_WhenMakingRequestToOpenApiJsonRoute_ThenSuccessResponseShouldBeReturned()
    {
        var response = await Client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();
    }
}