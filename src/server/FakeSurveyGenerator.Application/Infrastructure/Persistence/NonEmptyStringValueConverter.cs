using FakeSurveyGenerator.Application.Domain.Shared;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence;

internal class NonEmptyStringValueConverter() : ValueConverter<NonEmptyString, string>(domainValue => domainValue.Value,
    databaseValue => NonEmptyString.Create(databaseValue));