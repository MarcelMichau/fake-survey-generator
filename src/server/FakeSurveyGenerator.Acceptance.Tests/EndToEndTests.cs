using FluentAssertions;
using Microsoft.Playwright;
using Projects;
using Xunit.Abstractions;

namespace FakeSurveyGenerator.Acceptance.Tests;

public class EndToEndTests(ITestOutputHelper output)
{
    private const string UiProjectName = "fake-survey-generator-ui";

    // If Playwright fails with a "not installed" error, run the following command from the repo root directory:
    // pwsh .\src\server\FakeSurveyGenerator.Acceptance.Tests\bin\Debug\net9.0\playwright.ps1 install
    [Fact]
    public async Task GivenRunningApp_WhenOpeningUiWithPlaywright_ThenIndexPageIsDisplayed()
    {
        output.WriteLine("Running Playwright UI Index Page Test...");

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<FakeSurveyGenerator_Api>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        output.WriteLine("App Host Started");

        var httpClient = app.CreateHttpClient(UiProjectName);

        var playwright = await Playwright.CreateAsync();

        output.WriteLine("Launching Browser...");

        var browser = await playwright.Chromium.LaunchAsync();

        output.WriteLine("Browser Launched");

        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        var url = $"{httpClient.BaseAddress}";

        output.WriteLine($"Navigating to {url}");

        await page.GotoAsync(url);

        var title = await page.TextContentAsync("h1");
        title.Should().Be("Fake Survey Generator");

        output.WriteLine("Closing Browser...");
        await browser.CloseAsync();
        output.WriteLine("Browser Closed");
        playwright.Dispose();
    }
}