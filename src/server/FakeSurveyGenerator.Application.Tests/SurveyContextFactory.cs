using System;
using System.Collections.Generic;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.Common;
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
            var survey1 = new Survey(NonEmptyString.Create("Test Topic 1"), 10, NonEmptyString.Create("Testers"));
            var survey2 = new Survey(NonEmptyString.Create("Test Topic 2"), 20, NonEmptyString.Create("More Testers"));
            var survey3 = new Survey(NonEmptyString.Create("Test Topic 3"), 30, NonEmptyString.Create("Even More Testers"));

            survey1.AddSurveyOption(NonEmptyString.Create("Test Option 1"));

            survey2.AddSurveyOption(NonEmptyString.Create("Test Option 2"));
            survey2.AddSurveyOption(NonEmptyString.Create("Test Option 3"));

            survey3.AddSurveyOption(NonEmptyString.Create("Test Option 4"));
            survey3.AddSurveyOption(NonEmptyString.Create("Test Option 5"));
            survey3.AddSurveyOption(NonEmptyString.Create("Test Option 6"));

            context.Surveys.AddRange(new List<Survey> { survey1, survey2, survey3 });

            context.SaveChanges();
        }

        public static void Destroy(SurveyContext context)
        {
            context.Database.EnsureDeleted();

            context.Dispose();
        }
    }
}
