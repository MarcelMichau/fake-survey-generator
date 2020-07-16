using System.Data.Common;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace FakeSurveyGenerator.Infrastructure.Persistence
{
    //Courtest of this gist: https://gist.github.com/ChristopherHaws/b1c54b95838f1513bfb74fa1c8e408f3
    public static class AzureSqlServerConnectionExtensions
    {
        public static void UseAzureAccessToken(this DbContextOptionsBuilder options)
        {
            options.ReplaceService<ISqlServerConnection, AzureSqlServerConnection>();
        }
    }

    public class AzureSqlServerConnection : SqlServerConnection
    {
        // Compensate for slow SQL Server database creation
        private const int DefaultMasterConnectionCommandTimeout = 60;
        private static readonly AzureServiceTokenProvider TokenProvider = new AzureServiceTokenProvider();

        public AzureSqlServerConnection(RelationalConnectionDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override DbConnection CreateDbConnection() => new SqlConnection(this.ConnectionString)
        {
            // AzureServiceTokenProvider handles caching the token and refreshing it before it expires
            AccessToken = AsyncUtilities.RunSync(() => TokenProvider.GetAccessTokenAsync("https://database.windows.net/"))
        };

        public override ISqlServerConnection CreateMasterConnection()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(this.ConnectionString)
            {
                InitialCatalog = "master"
            };
            connectionStringBuilder.Remove("AttachDBFilename");

            var contextOptions = new DbContextOptionsBuilder()
                .UseSqlServer(
                    connectionStringBuilder.ConnectionString,
                    b => b.CommandTimeout(this.CommandTimeout ?? DefaultMasterConnectionCommandTimeout))
                .Options;

            return new AzureSqlServerConnection(this.Dependencies.With(contextOptions));
        }
    }
}
