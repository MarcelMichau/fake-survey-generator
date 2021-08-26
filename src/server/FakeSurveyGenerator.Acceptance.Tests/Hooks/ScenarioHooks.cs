using System.Threading.Tasks;
using BoDi;
using FakeSurveyGenerator.Acceptance.Tests.PageObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using TechTalk.SpecFlow;

namespace FakeSurveyGenerator.Acceptance.Tests.Hooks
{
    [Binding]
    public class ScenarioHooks
    {
        [BeforeScenario("LandingPage")]
        public async Task BeforeLandingPageScenario(IObjectContainer container)
        {
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync();
            var pageObject = new LandingPageObject(browser, LoadConfiguration());
            container.RegisterInstanceAs(playwright);
            container.RegisterInstanceAs(browser);
            container.RegisterInstanceAs(pageObject);
        }

        [AfterScenario]
        public async Task AfterScenario(IObjectContainer container)
        {
            var browser = container.Resolve<IBrowser>();
            await browser.CloseAsync();
            var playwright = container.Resolve<IPlaywright>();
            playwright.Dispose();
        }

        private static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }
    }
}
