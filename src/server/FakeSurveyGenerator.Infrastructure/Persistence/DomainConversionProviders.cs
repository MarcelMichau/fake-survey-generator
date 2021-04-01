using System;
using System.Linq.Expressions;
using FakeSurveyGenerator.Domain.Common;

namespace FakeSurveyGenerator.Infrastructure.Persistence
{
    public static class DomainConversionProviders
    {
        public static Expression<Func<NonEmptyString, string>>
            NonEmptyStringToString = domainValue => domainValue.Value;

        public static Expression<Func<string, NonEmptyString>>
            StringToNonEmptyString = databaseValue => NonEmptyString.Create(databaseValue);
    }
}
