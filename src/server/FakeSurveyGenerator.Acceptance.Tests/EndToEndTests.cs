using System.Collections.Concurrent;
using Microsoft.Playwright;
using TUnit.Playwright;

namespace FakeSurveyGenerator.Acceptance.Tests;

// Browser tests share one Chromium instance. A context created concurrently (with different TLS settings)
// appears to make Chromium drop in-flight connections with ERR_NETWORK_CHANGED, so never overlap them.
[NotInParallel("browser")]
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

    // Safety net: if Chromium still aborts every in-flight request with net::ERR_NETWORK_CHANGED (e.g. a host
    // network change outside the test run), the Vite shell is left without its modules.
    // Reload only for that failure, so genuine UI regressions still fail on the first attempt.
    // Each reload is recorded in ui-load-retries.log, which the pipeline surfaces as a build warning,
    // because output from passing tests is not retained anywhere visible.
    private static async Task GotoUntilRenderedAsync(IPage page, string url, int maxAttempts = 3)
    {
        const string networkChanged = "ERR_NETWORK_CHANGED";
        var failedUrls = new ConcurrentQueue<string>();
        page.RequestFailed += (_, request) =>
        {
            if (request.Failure?.Contains(networkChanged) == true) failedUrls.Enqueue(request.Url);
        };

        for (var attempt = 1; ; attempt++)
        {
            failedUrls.Clear();
            try
            {
                await page.GotoAsync(url);
                await page.Locator("h1").WaitForAsync();
                return;
            }
            catch (PlaywrightException exception) when (attempt < maxAttempts &&
                (!failedUrls.IsEmpty || exception.Message.Contains(networkChanged)))
            {
                var failed = failedUrls.IsEmpty ? url : string.Join(", ", failedUrls.Distinct());
                var message = $"UI load attempt {attempt} of {maxAttempts} hit {networkChanged}; reloading. Failed: {failed}";
                Console.WriteLine(message);
                await File.AppendAllTextAsync(
                    Path.Combine(TestContext.ResultsDirectory, "ui-load-retries.log"),
                    $"{DateTimeOffset.UtcNow:O} {message}{Environment.NewLine}");
            }
        }
    }
}
