using System;
using FakeSurveyGenerator.Application.Common.Interfaces;

namespace FakeSurveyGenerator.Infrastructure
{
    public class ConnectionString : IConnectionString
    {
        public string Value { get; }

        public ConnectionString(string connectionString)
        {
            Value = !string.IsNullOrWhiteSpace(connectionString) ? connectionString : throw new ArgumentNullException(nameof(connectionString));
        }
    }
}
