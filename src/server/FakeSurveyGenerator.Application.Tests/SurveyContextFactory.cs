using System;
using FakeSurveyGenerator.Data;
using FakeSurveyGenerator.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FakeSurveyGenerator.Application.Tests
{
    public static class SurveyContextFactory
    {
        public static SurveyContext Create()
        {
            var mockMediator = new Mock<IMediator>();

            var options = new DbContextOptionsBuilder<SurveyContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new SurveyContext(options, mockMediator.Object);

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
