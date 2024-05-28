using FakeSurveyGenerator.Api.Tests.Integration.Setup;
using Xunit.Abstractions;

namespace FakeSurveyGenerator.Api.Tests.Integration.Admin;

[Collection(nameof(IntegrationTestFixture))]
public sealed class AdminEndpointsTests(IntegrationTestFixture testFixture, ITestOutputHelper testOutputHelper)
{
    private readonly HttpClient _client = testFixture.Factory!.WithLoggerOutput(testOutputHelper).CreateClient();

    [Fact]
    public async Task Get_Should_Return_Ok()
    {
        var response = await _client.GetAsync("/api/admin/ping");
        response.EnsureSuccessStatusCode();
    }
}