using System.Collections.Generic;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;
using FakeSurveyGenerator.Domain.Common;
using FakeSurveyGenerator.Infrastructure.Persistence;

namespace FakeSurveyGenerator.Data
{
    public static class DatabaseSeed
    {
        public static void SeedSampleData(SurveyContext context)
        {
            var testOwner = new User(NonEmptyString.Create("Test User"), NonEmptyString.Create("test.user@test.com"), NonEmptyString.Create("test-id"));

            var survey1 = new Survey(testOwner, NonEmptyString.Create("Test Topic 1"), 10, NonEmptyString.Create("Testers"));
            var survey2 = new Survey(testOwner, NonEmptyString.Create("Test Topic 2"), 20, NonEmptyString.Create("More Testers"));
            var survey3 = new Survey(testOwner, NonEmptyString.Create("Test Topic 3"), 30, NonEmptyString.Create("Even More Testers"));

            survey1.AddSurveyOption(NonEmptyString.Create("Test Option 1"));

            survey2.AddSurveyOption(NonEmptyString.Create("Test Option 2"));
            survey2.AddSurveyOption(NonEmptyString.Create("Test Option 3"));

            survey3.AddSurveyOption(NonEmptyString.Create("Test Option 4"));
            survey3.AddSurveyOption(NonEmptyString.Create("Test Option 5"));
            survey3.AddSurveyOption(NonEmptyString.Create("Test Option 6"));

            context.Users.Add(testOwner);
            context.Surveys.AddRange(new List<Survey> { survey1, survey2, survey3 });

            context.SaveChanges();
        }
    }
}
