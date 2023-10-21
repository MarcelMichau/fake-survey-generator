using Microsoft.Data.SqlClient;

namespace FakeSurveyGenerator.Application.Shared.Persistence;

public interface IDatabaseConnection
{
    Task<SqlConnection> GetDbConnection();
}