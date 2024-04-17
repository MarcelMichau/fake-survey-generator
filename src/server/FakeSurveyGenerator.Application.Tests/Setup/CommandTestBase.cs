using FakeSurveyGenerator.Application.Infrastructure.Persistence;

namespace FakeSurveyGenerator.Application.Tests.Setup;

public class CommandTestBase : IDisposable
{
    protected SurveyContext Context { get; }

    protected CommandTestBase()
    {
        Context = SurveyContextFactory.Create();

        SurveyContextFactory.SeedSampleData(Context).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        SurveyContextFactory.Destroy(Context);
    }
}