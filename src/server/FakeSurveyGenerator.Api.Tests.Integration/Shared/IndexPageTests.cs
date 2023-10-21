using FakeSurveyGenerator.Api.Tests.Integration.Setup;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

[Collection(nameof(IntegrationTestFixture))]
public sealed class IndexPageTests(IntegrationTestFixture testFixture)
{
    private readonly HttpClient _client = testFixture.Factory!.CreateClient();

    [Fact]
    public async Task GivenAnyUser_WhenMakingRequestToRootRoute_ThenIndexPageShouldBeReturned()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
    }
}