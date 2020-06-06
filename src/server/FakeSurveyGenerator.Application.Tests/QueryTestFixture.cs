using System;
using AutoMapper;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests
{
    public sealed class QueryTestFixture : IDisposable
    {
        public SurveyContext Context { get; }

        public IMapper Mapper { get; }

        public IDistributedCache Cache { get; }

        public QueryTestFixture()
        {
            Context = SurveyContextFactory.Create();

            SurveyContextFactory.SeedSampleData(Context);

            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            Mapper = configurationProvider.CreateMapper();

            Cache = new Mock<IDistributedCache>().Object;
        }

        public void Dispose()
        {
            SurveyContextFactory.Destroy(Context);
        }
    }

    [CollectionDefinition("QueryTests")]
    public class QueryCollection : ICollectionFixture<QueryTestFixture> { }
}
