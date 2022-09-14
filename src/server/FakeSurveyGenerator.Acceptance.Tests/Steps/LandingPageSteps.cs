using System.Threading.Tasks;
using FakeSurveyGenerator.Acceptance.Tests.PageObjects;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace FakeSurveyGenerator.Acceptance.Tests.Steps;

[Binding]
public class LandingPageSteps
{
    private readonly LandingPageObject _pageObject;

    public LandingPageSteps(LandingPageObject pageObject)
    {
        _pageObject = pageObject;
    }

    [When(@"navigating to the application URL")]
    public async Task WhenNavigatingToTheApplicationURL()
    {
        await _pageObject.NavigateAsync();
    }
        
    [Then(@"the name of the application is displayed")]
    public async Task ThenTheNameOfTheApplicationIsDisplayed()
    {
        var content = await _pageObject.GetApplicationTitle();
        content.Should().Be("Fake Survey Generator");
    }

    [Then(@"the version number of the API is Displayed")]
    public async Task ThenTheVersionNumberOfTheAPIIsDisplayed()
    {
        //await _pageObject.WaitForApiCall();

        var content = await _pageObject.GetVersionInfo();

        content.Should().NotBe("Loading API Version...");
    }
}