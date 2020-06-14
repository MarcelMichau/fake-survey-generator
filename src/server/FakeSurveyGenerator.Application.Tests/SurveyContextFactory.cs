using System;
using System.Threading;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Data;
using FakeSurveyGenerator.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FakeSurveyGenerator.Application.Tests
{
    public static class SurveyContextFactory
    {
        public static SurveyContext Create()
        {
            var mockMediator = new Mock<IMediator>();

            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UnitTestUser());

            var options = new DbContextOptionsBuilder<SurveyContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new SurveyContext(options, mockMediator.Object, mockUserService.Object, new NullLogger<SurveyContext>());

            context.Database.EnsureCreated();

            return context;
        }

        public static void SeedSampleData(SurveyContext context)
        {
            DatabaseSeed.SeedSampleData(context);
        }

        public static void Destroy(SurveyContext context)
        {
            context.Database.EnsureDeleted();

            context.Dispose();
        }
    }
}
