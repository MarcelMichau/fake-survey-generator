using Microsoft.Playwright;
using Projects;

namespace FakeSurveyGenerator.Acceptance.Tests;

public class EndToEndTests
{
    private const string UiProjectName = "fake-survey-generator-ui";

    // If Playwright fails with a "not installed" error, run the following command from the repo root directory:
    // pwsh .\src\server\FakeSurveyGenerator.Acceptance.Tests\bin\Debug\net9.0\playwright.ps1 install
    [Test]
    public async Task GivenRunningApp_WhenOpeningUiWithPlaywright_ThenIndexPageIsDisplayed()
    {
        Console.WriteLine("Running Playwright UI Index Page Test...");

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<FakeSurveyGenerator_Api>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        Console.WriteLine("App Host Started");

        var httpClient = app.CreateHttpClient(UiProjectName);

        var playwright = await Playwright.CreateAsync();

        Console.WriteLine("Launching Browser...");

        var browser = await playwright.Chromium.LaunchAsync();

        Console.WriteLine("Browser Launched");

        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        var url = $"{httpClient.BaseAddress}";

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