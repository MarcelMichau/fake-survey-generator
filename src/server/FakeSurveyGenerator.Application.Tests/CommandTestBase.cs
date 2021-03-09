using System;
using System.Threading;
using AutoMapper;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Data;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Moq;

namespace FakeSurveyGenerator.Application.Tests
{
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

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TestUser());

            SurveyContextFactory.SeedSampleData(Context).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            SurveyContextFactory.Destroy(Context);
        }
    }
}
