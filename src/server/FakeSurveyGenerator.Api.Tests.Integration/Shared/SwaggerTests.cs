using FakeSurveyGenerator.Api.Tests.Integration.Setup;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

[Collection(nameof(IntegrationTestFixture))]
public sealed class SwaggerTests(IntegrationTestFixture testFixture)
{
    private readonly HttpClient _client = testFixture.Factory!.CreateClient();

    [Fact]
    public async Task GivenAnyUser_WhenMakingRequestToSwaggerUiRoute_ThenSuccessResponseShouldBeReturned()
    {
        var response = await _client.GetAsync("/swagger");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GivenAnyUser_WhenMakingRequestToSwaggerJsonRoute_ThenSuccessResponseShouldBeReturned()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
    }
}