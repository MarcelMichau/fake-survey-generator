using System.Collections.Generic;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Infrastructure;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    public static class DatabaseSeed
    {
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
    }
}
