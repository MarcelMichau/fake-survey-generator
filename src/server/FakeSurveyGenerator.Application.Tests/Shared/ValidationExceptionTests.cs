using FakeSurveyGenerator.Application.Shared.Exceptions;
using FluentValidation.Results;

namespace FakeSurveyGenerator.Application.Tests.Shared;

public sealed class ValidationExceptionTests
{
    [Test]
    public async Task DefaultConstructorCreatesAnEmptyErrorDictionary()
    {
        var actual = new ValidationException().Errors;

        await Assert.That(actual.Keys).IsEquivalentTo(Array.Empty<string>());
    }

    [Test]
    public async Task SingleValidationFailureCreatesASingleElementErrorDictionary()
    {
        var failures = new List<ValidationFailure>
        {
            new("Age", "must be over 18")
        };

        var actual = new ValidationException(failures).Errors;

        await Assert.That(actual.Keys).IsEquivalentTo(new[] { "Age" });
        await Assert.That(actual["Age"]).IsEquivalentTo(new [] { "must be over 18" });
    }

    [Test]
    public async Task
        MultipleValidationFailureForMultiplePropertiesCreatesAMultipleElementErrorDictionaryEachWithMultipleValues()
    {
        var failures = new List<ValidationFailure>
        {
            new("Age", "must be 18 or older"),
            new("Age", "must be 25 or younger"),
            new("Password", "must contain at least 8 characters"),
            new("Password", "must contain a digit"),
            new("Password", "must contain upper case letter"),
            new("Password", "must contain lower case letter")
        };

        var actual = new ValidationException(failures).Errors;

        await Assert.That(actual.Keys).IsEquivalentTo(new[] { "Age", "Password" });

        await Assert.That(actual["Age"]).IsEquivalentTo(new[] { "must be 18 or older", "must be 25 or younger" });

        await Assert.That(actual["Password"]).IsEquivalentTo(new[]
        {
            "must contain at least 8 characters", "must contain a digit",
            "must contain upper case letter", "must contain lower case letter"
        });
    }
}