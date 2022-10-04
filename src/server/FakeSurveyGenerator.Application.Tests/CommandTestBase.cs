using AutoMapper;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Infrastructure.Persistence;

namespace FakeSurveyGenerator.Application.Tests;

public class CommandTestBase : IDisposable
{
    protected SurveyContext Context { get; }
    protected IMapper Mapper { get; }

    protected CommandTestBase()
    {
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        Mapper = configurationProvider.CreateMapper();
        Context = SurveyContextFactory.Create();

        SurveyContextFactory.SeedSampleData(Context).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        SurveyContextFactory.Destroy(Context);
    }
}