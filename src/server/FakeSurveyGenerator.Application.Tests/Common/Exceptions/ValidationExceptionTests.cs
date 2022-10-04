using FakeSurveyGenerator.Application.Common.Exceptions;
using FluentValidation.Results;
using FluentAssertions;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Common.Exceptions;

public sealed class ValidationExceptionTests
{
    [Fact]
    public void DefaultConstructorCreatesAnEmptyErrorDictionary()
    {
        var actual = new ValidationException().Errors;

        actual.Keys.Should().BeEquivalentTo(Array.Empty<string>());
    }

    [Fact]
    public void SingleValidationFailureCreatesASingleElementErrorDictionary()
    {
        var failures = new List<ValidationFailure>
        {
            new("Age", "must be over 18")
        };

        var actual = new ValidationException(failures).Errors;

        actual.Keys.Should().BeEquivalentTo("Age");
        actual["Age"].Should().BeEquivalentTo("must be over 18");
    }

    [Fact]
    public void MultipleValidationFailureForMultiplePropertiesCreatesAMultipleElementErrorDictionaryEachWithMultipleValues()
    {
        var failures = new List<ValidationFailure>
        {
            new("Age", "must be 18 or older"),
            new("Age", "must be 25 or younger"),
            new("Password", "must contain at least 8 characters"),
            new("Password", "must contain a digit"),
            new("Password", "must contain upper case letter"),
            new("Password", "must contain lower case letter"),
        };

        var actual = new ValidationException(failures).Errors;

        actual.Keys.Should().BeEquivalentTo("Age", "Password");

        actual["Age"].Should().BeEquivalentTo("must be 18 or older", "must be 25 or younger");

        actual["Password"].Should().BeEquivalentTo("must contain at least 8 characters", "must contain a digit", "must contain upper case letter", "must contain lower case letter");
    }
}