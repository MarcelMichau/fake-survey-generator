using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace FakeSurveyGenerator.Acceptance.Tests.PageObjects
{
    public class LandingPageObject : BasePageObject
    {
        public override string PagePath => $"{BaseAddress}";
        public override IPage Page { get; set; }
        public override IBrowserContext Context { get; }

        public LandingPageObject(IBrowserContext browser, IConfiguration configuration) : base(configuration)
        {
            Context = browser;
        }

        public async Task<string> GetApplicationTitle()
        {
            return await Page.TextContentAsync("h1");
        }

        public async Task<string> GetVersionInfo()
        {
            return await Page.TextContentAsync("[data-test=version-info]");
        }
    }
}
