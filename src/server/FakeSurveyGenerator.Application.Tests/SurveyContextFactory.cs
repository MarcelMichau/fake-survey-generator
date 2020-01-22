using System.Collections.Generic;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Tests
{
    public static class SurveyContextFactory
    {
        public static SurveyContext Create()
        {
            var options = new DbContextOptionsBuilder<SurveyContext>()
                .UseInMemoryDatabase("InMemoryDbForTesting")
                .Options;

            var context = new SurveyContext(options);

            context.Database.EnsureCreated();

            return context;
        }

        public static void SeedSampleData(SurveyContext context)
        {
            var survey1 = new Survey("Test Topic 1", 10, "Testers");
            var survey2 = new Survey("Test Topic 2", 20, "More Testers");
            var survey3 = new Survey("Test Topic 3", 30, "Even More Testers");

            survey1.AddSurveyOption("Test Option 1");

            survey2.AddSurveyOption("Test Option 2");
            survey2.AddSurveyOption("Test Option 3");

            survey3.AddSurveyOption("Test Option 4");
            survey3.AddSurveyOption("Test Option 5");
            survey3.AddSurveyOption("Test Option 6");

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
