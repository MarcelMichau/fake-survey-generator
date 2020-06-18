using System;

namespace FakeSurveyGenerator.Domain.Exceptions
{
    public sealed class SurveyDomainException : Exception
    {
        public SurveyDomainException()
        { }

        public SurveyDomainException(string message)
            : base(message)
        { }

        public SurveyDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
