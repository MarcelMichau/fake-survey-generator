using Microsoft.Playwright;
using TUnit.Playwright;

namespace FakeSurveyGenerator.Acceptance.Tests;

public class EndToEndTests : ContextTest
{
    [ClassDataSource<AcceptanceTestFixture>(Shared = SharedType.PerTestSession)]
    public required AcceptanceTestFixture TestFixture { get; init; }
    private HttpClient UiClient => TestFixture.App!.CreateHttpClient("ui", "https");

    public override BrowserNewContextOptions ContextOptions(TestContext testContext)
    {
        return new BrowserNewContextOptions
        {
            // Playwright only seems to fail with ERR_CERT_AUTHORITY_INVALID in CI environments
            // This is here to work around that issue
            IgnoreHTTPSErrors = true
        };
    }

    // If Playwright fails with a "not installed" error, run the following command from the repo root directory:
    // pwsh .\src\server\FakeSurveyGenerator.Acceptance.Tests\bin\Debug\net10.0\playwright.ps1 install
    [Test]
    public async Task GivenRunningApp_WhenOpeningUiWithPlaywright_ThenIndexPageIsDisplayed(CancellationToken cancellationToken)
    {
        Console.WriteLine("Running Playwright UI Index Page Test...");

        var url = $"{UiClient.BaseAddress}";

        Console.WriteLine($"Navigating to {url}");

        var page = await Context.NewPageAsync();

        await page.GotoAsync(url);

        var title = await page.TextContentAsync("h1");
        await Assert.That(title).IsEqualTo("Fake Survey Generator");
    }
}