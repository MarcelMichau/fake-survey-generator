using System;

namespace FakeSurveyGenerator.Domain.Common
{
    public sealed record NonEmptyString
    {
        public string Value { get; }

        private NonEmptyString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(
                    $"Cannot assign an empty string value to a {nameof(NonEmptyString)}");

            Value = value;
        }

        public static NonEmptyString Create(string value)
        {
            return new NonEmptyString(value);
        }

        public static explicit operator NonEmptyString(string value)
        {
            return new NonEmptyString(value);
        }

        public static implicit operator string(NonEmptyString value)
        {
            return value.Value;
        }
    }
}
