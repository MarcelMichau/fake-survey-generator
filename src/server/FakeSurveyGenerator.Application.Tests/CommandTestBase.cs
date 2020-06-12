using System;
using AutoMapper;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Infrastructure.Persistence;

namespace FakeSurveyGenerator.Application.Tests
{
    public class CommandTestBase : IDisposable
    {
        public SurveyContext Context { get; }
        public IMapper Mapper { get; }
        public IUser User { get; }

        public CommandTestBase()
        {
            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            Mapper = configurationProvider.CreateMapper();
            Context = SurveyContextFactory.Create();
            User = new UnitTestUser();

            SurveyContextFactory.SeedSampleData(Context);
        }

        public void Dispose()
        {
            SurveyContextFactory.Destroy(Context);
        }
    }
}
