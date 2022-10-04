using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration.Controllers;

[Collection(nameof(IntegrationTestFixture))]
public sealed class AdminControllerTests
{
    private readonly HttpClient _client;

    public AdminControllerTests(IntegrationTestFixture testFixture)
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