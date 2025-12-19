using TUnit.Playwright;

namespace FakeSurveyGenerator.Acceptance.Tests;

public class EndToEndTests : PageTest
{
    [ClassDataSource<AcceptanceTestFixture>(Shared = SharedType.PerTestSession)]
    public required AcceptanceTestFixture TestFixture { get; init; }
    private HttpClient UiClient => TestFixture.App!.CreateHttpClient("ui", "https");

    // If Playwright fails with a "not installed" error, run the following command from the repo root directory:
    // pwsh .\src\server\FakeSurveyGenerator.Acceptance.Tests\bin\Debug\net10.0\playwright.ps1 install
    [Test]
    public async Task GivenRunningApp_WhenOpeningUiWithPlaywright_ThenIndexPageIsDisplayed(CancellationToken cancellationToken)
    {
        Console.WriteLine("Running Playwright UI Index Page Test...");

        var url = $"{UiClient.BaseAddress}";

        Console.WriteLine($"Navigating to {url}");

        await Page.GotoAsync(url);

        var title = await Page.TextContentAsync("h1");
        await Assert.That(title).IsEqualTo("Fake Survey Generator");
    }
}