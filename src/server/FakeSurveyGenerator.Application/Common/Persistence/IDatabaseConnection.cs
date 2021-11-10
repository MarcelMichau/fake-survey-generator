using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace FakeSurveyGenerator.Application.Common.Persistence;

public interface IDatabaseConnection
{
    Task<SqlConnection> GetDbConnection();
}