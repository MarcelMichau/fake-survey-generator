using FakeSurveyGenerator.Api.Tests.Integration.Setup;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

public sealed class SwaggerTests
{
    [ClassDataSource<IntegrationTestFixture>(Shared = SharedType.PerTestSession)]
    public required IntegrationTestFixture TestFixture { get; init; }

    private HttpClient Client => TestFixture.Factory!.CreateClient();

    [Test]
    public async Task GivenAnyUser_WhenMakingRequestToSwaggerUiRoute_ThenSuccessResponseShouldBeReturned()
    {
        var response = await Client.GetAsync("/swagger");
        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task GivenAnyUser_WhenMakingRequestToSwaggerJsonRoute_ThenSuccessResponseShouldBeReturned()
    {
        var response = await Client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
    }
}