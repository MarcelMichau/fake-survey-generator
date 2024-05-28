using FakeSurveyGenerator.Api.Tests.Integration.Setup;
using Xunit.Abstractions;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

[Collection(nameof(IntegrationTestFixture))]
public sealed class IndexPageTests(IntegrationTestFixture testFixture, ITestOutputHelper testOutputHelper)
{
    private readonly HttpClient _client = testFixture.Factory!.WithLoggerOutput(testOutputHelper).CreateClient();

    [Fact]
    public async Task GivenAnyUser_WhenMakingRequestToRootRoute_ThenIndexPageShouldBeReturned()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
    }
}