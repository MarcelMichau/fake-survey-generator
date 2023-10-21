using FakeSurveyGenerator.Application.Shared.Persistence;
using Microsoft.Data.SqlClient;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence;

internal sealed class DapperSqlServerConnection(string connectionString) : IDatabaseConnection
{
    private readonly string _connectionString = !string.IsNullOrWhiteSpace(connectionString) ? connectionString : throw new ArgumentNullException(nameof(connectionString));

    public Task<SqlConnection> GetDbConnection()
    {
        return Task.FromResult(new SqlConnection(_connectionString));
    }
}