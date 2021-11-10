using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration.Common;

[Collection(nameof(IntegrationTestFixture))]
public sealed class IndexPageTests
{
    private readonly HttpClient _client;

    public IndexPageTests(IntegrationTestFixture testFixture)
    {
        _client = testFixture.Factory.CreateClient();
    }

    [Fact]
    public async Task GivenAnyUser_WhenMakingRequestToRootRoute_ThenIndexPageShouldBeReturned()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
    }
}