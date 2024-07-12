using FakeSurveyGenerator.Application.Infrastructure.Persistence;

namespace FakeSurveyGenerator.Application.Tests.Setup;

public class CommandTestBase : IDisposable
{
    protected CommandTestBase()
    {
        Context = SurveyContextFactory.Create();

        SurveyContextFactory.SeedSampleData(Context).GetAwaiter().GetResult();
    }

    protected SurveyContext Context { get; }

    public void Dispose()
    {
        SurveyContextFactory.Destroy(Context);
    }
}