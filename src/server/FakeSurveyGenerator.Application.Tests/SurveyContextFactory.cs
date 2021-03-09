using System;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.DomainEvents;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Data;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FakeSurveyGenerator.Application.Tests
{
    public static class SurveyContextFactory
    {
        public static SurveyContext Create()
        {
            var mockDomainEventService = new Mock<IDomainEventService>();

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TestUser());

            var options = new DbContextOptionsBuilder<SurveyContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new SurveyContext(options, mockDomainEventService.Object, mockUserService.Object, new NullLogger<SurveyContext>());

            context.Database.EnsureCreated();

            return context;
        }

        public static async Task SeedSampleData(SurveyContext context)
        {
            await DatabaseSeed.SeedSampleData(context);
        }

        public static void Destroy(SurveyContext context)
        {
            context.Database.EnsureDeleted();

            context.Dispose();
        }
    }
}
