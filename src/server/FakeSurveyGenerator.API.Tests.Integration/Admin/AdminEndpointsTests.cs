using FakeSurveyGenerator.API.Tests.Integration.Setup;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration.Admin;

[Collection(nameof(IntegrationTestFixture))]
public sealed class AdminEndpointsTests
{
    private readonly HttpClient _client;

    public AdminEndpointsTests(IntegrationTestFixture testFixture)
    {
        _client = testFixture.Factory!.CreateClient();
    }

    [Fact]
    public async Task Get_Should_Return_Ok()
    {
        var response = await _client.GetAsync("/api/admin/ping");
        response.EnsureSuccessStatusCode();
    }
}