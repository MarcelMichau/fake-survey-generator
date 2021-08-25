using System.Threading.Tasks;
using Microsoft.Playwright;

namespace FakeSurveyGenerator.Acceptance.Tests.PageObjects
{
    public class LandingPageObject : BasePageObject
    {
        public override string PagePath => "https://127.0.0.1:3000";
        public override IPage Page { get; set; }
        public override IBrowser Browser { get; }

        public LandingPageObject(IBrowser browser)
        {
            Browser = browser;
        }

        public async Task<string> GetApplicationTitle()
        {
            return await Page.TextContentAsync("h1");
        }

        public async Task WaitForApiCall()
        {
            await Page.WaitForResponseAsync(r => r.Url.Contains("version")); ;
        }

        public async Task<string> GetVersionInfo()
        {
            return await Page.TextContentAsync("[data-test=version-info]");
        }
    }
}
