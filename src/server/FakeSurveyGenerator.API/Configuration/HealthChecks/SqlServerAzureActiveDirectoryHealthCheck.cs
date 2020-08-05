using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FakeSurveyGenerator.API.Configuration.HealthChecks
{
    internal class SqlServerAzureActiveDirectoryHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _sql;

        public SqlServerAzureActiveDirectoryHealthCheck(string sqlServerConnectionString, string sql)
        {
            _connectionString = sqlServerConnectionString ??
                                throw new ArgumentNullException(nameof(sqlServerConnectionString));
            _sql = sql ?? throw new ArgumentNullException(nameof(sql));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var serviceTokenProvider = new Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider();
                var accessToken = await serviceTokenProvider
                    .GetAccessTokenAsync("https://database.windows.net/", cancellationToken: cancellationToken);

                await using var connection = new SqlConnection(_connectionString) {AccessToken = accessToken};
                await connection.OpenAsync(cancellationToken);

                await using (var command = connection.CreateCommand())
                {
                    command.CommandText = _sql;
                    await command.ExecuteScalarAsync(cancellationToken);
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }

    public static class SqlServerHealthCheckBuilderExtensions
    {
        private const string HEALTH_QUERY = "SELECT 1;";
        private const string NAME = "sqlserver";

        /// <summary>
        /// Add a health check for SqlServer services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The Sql Server connection string to be used.</param>
        /// <param name="healthQuery">The query to be executed.Optional. If <c>null</c> select 1 is used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sqlserver' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddSqlServerWithAzureAd(this IHealthChecksBuilder builder,
            string connectionString,
            string healthQuery = default,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            return builder.AddSqlServerWithAzureAd(_ => connectionString, healthQuery, name, failureStatus, tags, timeout);
        }

        /// <summary>
        /// Add a health check for SqlServer services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionStringFactory">A factory to build the SQL Server connection string to use.</param>
        /// <param name="healthQuery">The query to be executed.Optional. If <c>null</c> select 1 is used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sqlserver' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddSqlServerWithAzureAd(this IHealthChecksBuilder builder,
            Func<IServiceProvider,
                string> connectionStringFactory,
            string healthQuery = default,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default,
            TimeSpan? timeout = default)
        {
            if (connectionStringFactory == null)
            {
                throw new ArgumentNullException(nameof(connectionStringFactory));
            }

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new SqlServerAzureActiveDirectoryHealthCheck(connectionStringFactory(sp), healthQuery ?? HEALTH_QUERY),
                failureStatus,
                tags,
                timeout));
        }
    }
}