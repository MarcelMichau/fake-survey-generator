using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace FakeSurveyGenerator.Acceptance.Tests.PageObjects;

public abstract class BasePageObject
{
    public string BaseAddress { get; }
    public abstract string PagePath { get; }
    public abstract IPage Page { get; set; }
    public abstract IBrowserContext Context { get; }

    protected BasePageObject(IConfiguration configuration)
    {
        BaseAddress = configuration.GetValue<string>("FakeSurveyGeneratorUI:BaseAddress");
    }

    public async Task NavigateAsync()
    {
        Page = await Context.NewPageAsync();
        await Page.GotoAsync(PagePath);
    }

    public async Task NavigateAndWaitForResponseAsync(Func<IResponse, bool> responsePredicate)
    {
        Page = await Context.NewPageAsync();

        await Page.RunAndWaitForResponseAsync(async () =>
        {
            await Page.GotoAsync(PagePath);
        }, responsePredicate);
    }
}