using FakeSurveyGenerator.Api.Tests.Integration.Setup;

namespace FakeSurveyGenerator.Api.Tests.Integration.Admin;

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