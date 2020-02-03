using System;
using AutoMapper;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Infrastructure;

namespace FakeSurveyGenerator.Application.Tests
{
    public class CommandTestBase : IDisposable
    {
        public SurveyContext Context;
        public IMapper Mapper;

        public CommandTestBase()
        {
            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            Mapper = configurationProvider.CreateMapper();
            Context = SurveyContextFactory.Create();
        }

        public void Dispose()
        {
            SurveyContextFactory.Destroy(Context);
        }
    }
}
