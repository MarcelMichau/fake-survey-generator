using System;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Persistence;
using Microsoft.Data.SqlClient;

namespace FakeSurveyGenerator.Infrastructure.Persistence
{
    internal sealed class DapperSqlServerConnection : IDatabaseConnection
    {
        private readonly string _connectionString;
 
        public DapperSqlServerConnection(string connectionString)
        {
            _connectionString = !string.IsNullOrWhiteSpace(connectionString) ? connectionString : throw new ArgumentNullException(nameof(connectionString));
        }
 
        public async Task<SqlConnection> GetDbConnection()
        {
            return new(_connectionString);
        }
    }
}
