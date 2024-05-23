using FakeSurveyGenerator.Api.Tests.Integration.Setup;

namespace FakeSurveyGenerator.Api.Tests.Integration.Admin;

[Collection(nameof(IntegrationTestFixture))]
public sealed class AdminEndpointsTests(IntegrationTestFixture testFixture)
{
    private readonly HttpClient _client = testFixture.Factory!.CreateClient();

    [Fact]
    public async Task Get_Should_Return_Ok()
    {
        var response = await _client.GetAsync("/api/admin/ping");
        response.EnsureSuccessStatusCode();
    }
}