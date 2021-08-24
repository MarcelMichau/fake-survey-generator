using System.Threading.Tasks;
using BoDi;
using FakeSurveyGenerator.Acceptance.Tests.PageObjects;
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
            var pageObject = new LandingPageObject(browser);
            container.RegisterInstanceAs(playwright);
            container.RegisterInstanceAs(browser);
            container.RegisterInstanceAs(pageObject);
        }

        [AfterScenario()]
        public async Task AfterScenario(IObjectContainer container)
        {
            var browser = container.Resolve<IBrowser>();
            await browser.CloseAsync();
            var playwright = container.Resolve<IPlaywright>();
            playwright.Dispose();
        }
    }
}
