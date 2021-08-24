using System.Threading.Tasks;
using Microsoft.Playwright;

namespace FakeSurveyGenerator.Acceptance.Tests.PageObjects
{
    public abstract class BasePageObject
    {
        public abstract string PagePath { get; }
        public abstract IPage Page { get; set; }
        public abstract IBrowser Browser { get; }

        private readonly BrowserNewPageOptions _browserNewPageOptions = new()
        {
            IgnoreHTTPSErrors = true
        };

        public async Task NavigateAsync()
        {
            Page = await Browser.NewPageAsync(_browserNewPageOptions);
            await Page.GotoAsync(PagePath);
        }
    }
}
