using FakeSurveyGenerator.Api.Tests.Integration.Setup;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

public sealed class IndexPageTests
{
    [ClassDataSource<IntegrationTestFixture>(Shared = SharedType.PerTestSession)]
    public required IntegrationTestFixture TestFixture { get; init; }

    private HttpClient Client => TestFixture.Factory!.CreateClient();

    [Test]
    public async Task GivenAnyUser_WhenMakingRequestToRootRoute_ThenIndexPageShouldBeReturned()
    {
        var response = await Client.GetAsync("/");
        response.EnsureSuccessStatusCode();
    }
}