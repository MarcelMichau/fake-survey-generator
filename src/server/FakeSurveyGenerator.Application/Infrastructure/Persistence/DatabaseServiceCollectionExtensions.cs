using FakeSurveyGenerator.Application.Infrastructure.Persistence.Interceptors;
using FakeSurveyGenerator.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence;

internal static class DatabaseServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddDatabaseConfiguration(this IHostApplicationBuilder builder,
        IConfiguration configuration)
    {
        const string connectionName = "database";

        var connectionString = configuration.GetConnectionString(connectionName) ??
                               throw new InvalidOperationException(
                                   $"Connection String for '{connectionName}' was not found in config");

        builder.Services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        builder.Services.AddDbContext<SurveyContext>
        (options =>
            {
                options.UseSqlServer(connectionString);
            }
        );

        builder.EnrichSqlServerDbContext<SurveyContext>();

        builder.Services.AddScoped<IDatabaseConnection>(_ => new DapperSqlServerConnection(connectionString));

        return builder;
    }
}