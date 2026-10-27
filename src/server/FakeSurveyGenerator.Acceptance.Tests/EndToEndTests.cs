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
    // pwsh .\src\server\FakeSurveyGenerator.Acceptance.Tests\bin\Debug\net11.0\playwright.ps1 install
    [Test]
    public async Task GivenRunningApp_WhenOpeningUiWithPlaywright_ThenIndexPageIsDisplayed(CancellationToken cancellationToken)
    {
        Console.WriteLine("Running Playwright UI Index Page Test...");

        var url = $"{UiClient.BaseAddress}";

        Console.WriteLine($"Navigating to {url}");

        await UiTestDiagnostics.RunAsync(Context, async page =>
        {
            await GotoUntilRenderedAsync(page, url);

            var title = await page.TextContentAsync("h1");
            await Assert.That(title).IsEqualTo("Fake Survey Generator");
        });
    }

    // On CI agents, Chromium intermittently aborts every in-flight request with net::ERR_NETWORK_CHANGED
    // (e.g. when containers alter the host's network interfaces), leaving the Vite shell without its modules.
    // Reload only for that failure, so genuine UI regressions still fail on the first attempt.
    private static async Task GotoUntilRenderedAsync(IPage page, string url, int maxAttempts = 3)
    {
        const string networkChanged = "ERR_NETWORK_CHANGED";
        var networkChangedDuringAttempt = false;
        page.RequestFailed += (_, request) =>
        {
            if (request.Failure?.Contains(networkChanged) == true) networkChangedDuringAttempt = true;
        };

        for (var attempt = 1; ; attempt++)
        {
            networkChangedDuringAttempt = false;
            try
            {
                await page.GotoAsync(url);
                await page.Locator("h1").WaitForAsync();
                return;
            }
            catch (PlaywrightException exception) when (attempt < maxAttempts &&
                (networkChangedDuringAttempt || exception.Message.Contains(networkChanged)))
            {
                Console.WriteLine($"UI load attempt {attempt} hit {networkChanged}; reloading.");
            }
        }
    }
}
