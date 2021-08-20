using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace FakeSurveyGenerator.UI.Tests;
public class LandingPageTests
{
    private const string pageUrl = "https://localhost:3000";

    private readonly BrowserTypeLaunchOptions _browserTypeLaunchOptions = new()
    {
        Channel = "msedge-dev",
        //Headless = false,
        //SlowMo = 1000,
    };

    private readonly BrowserNewPageOptions _browserNewPageOptions = new()
    {
        IgnoreHTTPSErrors = true
    };

    [Fact]
    public async Task GivenIndexPage_WhenLoadingPage_HeadingShouldShowApplicationName()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(_browserTypeLaunchOptions);

        var page = await browser.NewPageAsync(_browserNewPageOptions);

        await page.GotoAsync(pageUrl);
        var content = await page.TextContentAsync("h1");
        content.Should().Be("Fake Survey Generator");
    }

    [Fact]
    public async Task GivenIndexPage_WhenLoadingPage_ApiVersionShouldBeDisplayed()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(_browserTypeLaunchOptions);

        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions {
            IgnoreHTTPSErrors = true
        });

        var page = await context.NewPageAsync();

        await page.GotoAsync(pageUrl);
        var response = await page.WaitForResponseAsync(r => r.Url.Contains("version"));

        var content = await page.TextContentAsync("[data-test=version-info]");

        content.Should().NotBe("Loading API Version...");
    }
}