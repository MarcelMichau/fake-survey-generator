using FakeSurveyGenerator.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FakeSurveyGenerator.Infrastructure.Persistence;

internal class NonEmptyStringValueConverter : ValueConverter<NonEmptyString, string>
{
    public NonEmptyStringValueConverter() : base(domainValue => domainValue.Value,
        databaseValue => NonEmptyString.Create(databaseValue))
    {
    }
}