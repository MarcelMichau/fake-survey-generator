using FakeSurveyGenerator.Api.Tests.Integration.Setup;
using Xunit.Abstractions;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

[Collection(nameof(IntegrationTestFixture))]
public sealed class SwaggerTests(IntegrationTestFixture testFixture, ITestOutputHelper testOutputHelper)
{
    private readonly HttpClient _client = testFixture.Factory!.WithLoggerOutput(testOutputHelper).CreateClient();

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