using Microsoft.Playwright;

namespace FakeSurveyGenerator.Acceptance.Tests;

public class EndToEndTests
{
    private const string UiProjectName = "fake-survey-generator-ui";

    [ClassDataSource<AcceptanceTestFixture>(Shared = SharedType.PerTestSession)]
    public required AcceptanceTestFixture TestFixture { get; init; }
    private HttpClient UiClient => TestFixture.App!.CreateHttpClient(UiProjectName);

    // If Playwright fails with a "not installed" error, run the following command from the repo root directory:
    // pwsh .\src\server\FakeSurveyGenerator.Acceptance.Tests\bin\Debug\net9.0\playwright.ps1 install
    [Test]
    public async Task GivenRunningApp_WhenOpeningUiWithPlaywright_ThenIndexPageIsDisplayed()
    {
        Console.WriteLine("Running Playwright UI Index Page Test...");

        var playwright = await Playwright.CreateAsync();

        Console.WriteLine("Launching Browser...");

        var browser = await playwright.Chromium.LaunchAsync();

        Console.WriteLine("Browser Launched");

        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        var url = $"{UiClient.BaseAddress}";

        Console.WriteLine($"Navigating to {url}");

        await page.GotoAsync(url);

        var title = await page.TextContentAsync("h1");
        await Assert.That(title).IsEqualTo("Fake Survey Generator");

        Console.WriteLine("Closing Browser...");
        await browser.CloseAsync();
        Console.WriteLine("Browser Closed");
        playwright.Dispose();
    }
}