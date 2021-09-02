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
        private static IConfiguration _configuration;

        public ScenarioHooks()
        {
            _configuration = LoadConfiguration();
        }

        [BeforeScenario("LandingPage")]
        public async Task BeforeLandingPageScenario(IObjectContainer container)
        {
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync();

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true
            });

            if (_configuration.GetValue<bool>("EnableTracing"))
            {
                await context.Tracing.StartAsync(new TracingStartOptions
                {
                    Screenshots = true,
                    Snapshots = true
                });
            }

            var pageObject = new LandingPageObject(context, _configuration);
            container.RegisterInstanceAs(playwright);
            container.RegisterInstanceAs(browser);
            container.RegisterInstanceAs(context);
            container.RegisterInstanceAs(pageObject);
        }

        [AfterScenario]
        public async Task AfterScenario(IObjectContainer container)
        {
            var context = container.Resolve<IBrowserContext>();

            if (_configuration.GetValue<bool>("EnableTracing"))
            {
                await context.Tracing.StopAsync(new TracingStopOptions
                {
                    Path = "trace.zip"
                });
            }

            await context.CloseAsync();

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
