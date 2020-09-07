using System;
using System.Collections.Generic;
using FakeSurveyGenerator.Application.Common.Exceptions;
using FluentValidation.Results;
using Shouldly;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Common.Exceptions
{
    public sealed class ValidationExceptionTests
    {
        [Fact]
        public void DefaultConstructorCreatesAnEmptyErrorDictionary()
        {
            var actual = new ValidationException().Errors;

            actual.Keys.ShouldBe(Array.Empty<string>());
        }

        [Fact]
        public void SingleValidationFailureCreatesASingleElementErrorDictionary()
        {
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Age", "must be over 18"),
            };

            var actual = new ValidationException(failures).Errors;

            actual.Keys.ShouldBe(new[] {"Age"});
            actual["Age"].ShouldBe(new[] {"must be over 18"});
        }

        [Fact]
        public void MultipleValidationFailureForMultiplePropertiesCreatesAMultipleElementErrorDictionaryEachWithMultipleValues()
        {
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Age", "must be 18 or older"),
                new ValidationFailure("Age", "must be 25 or younger"),
                new ValidationFailure("Password", "must contain at least 8 characters"),
                new ValidationFailure("Password", "must contain a digit"),
                new ValidationFailure("Password", "must contain upper case letter"),
                new ValidationFailure("Password", "must contain lower case letter"),
            };

            var actual = new ValidationException(failures).Errors;

            actual.Keys.ShouldBe(new[] {"Age", "Password"});

            actual["Age"].ShouldBe(new[]
            {
                "must be 18 or older",
                "must be 25 or younger"
            });

            actual["Password"].ShouldBe(new[]
            {
                "must contain at least 8 characters",
                "must contain a digit",
                "must contain upper case letter",
                "must contain lower case letter"
            });
        }
    }
}