using FakeSurveyGenerator.Api.Tests.Integration.Setup;

namespace FakeSurveyGenerator.Api.Tests.Integration.Admin;

public sealed class AdminEndpointsTests
{
    [ClassDataSource<IntegrationTestFixture>(Shared = SharedType.PerTestSession)]
    public required IntegrationTestFixture TestFixture { get; init; }

    private HttpClient Client => TestFixture.Factory!.CreateClient();

    [Test]
    public async Task Get_Should_Return_Ok()
    {
        var response = await Client.GetAsync("/api/admin/ping");
        response.EnsureSuccessStatusCode();
    }
}