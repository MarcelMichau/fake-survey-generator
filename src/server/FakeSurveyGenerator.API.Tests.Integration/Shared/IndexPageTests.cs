using FakeSurveyGenerator.API.Tests.Integration.Setup;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration.Shared;

[Collection(nameof(IntegrationTestFixture))]
public sealed class IndexPageTests
{
    private readonly HttpClient _client;

    public IndexPageTests(IntegrationTestFixture testFixture)
    {
        _client = testFixture.Factory!.CreateClient();
    }

    [Fact]
    public async Task GivenAnyUser_WhenMakingRequestToRootRoute_ThenIndexPageShouldBeReturned()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
    }
}