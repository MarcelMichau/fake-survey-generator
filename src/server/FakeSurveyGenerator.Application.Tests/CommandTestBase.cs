using System;
using System.Threading;
using AutoMapper;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Moq;

namespace FakeSurveyGenerator.Application.Tests
{
    public class CommandTestBase : IDisposable
    {
        public SurveyContext Context { get; }
        public IMapper Mapper { get; }
        public IUserService UserService { get; }

        public CommandTestBase()
        {
            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            Mapper = configurationProvider.CreateMapper();
            Context = SurveyContextFactory.Create();

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UnitTestUser());

            UserService = mockUserService.Object;

            SurveyContextFactory.SeedSampleData(Context);
        }

        public void Dispose()
        {
            SurveyContextFactory.Destroy(Context);
        }
    }
}
