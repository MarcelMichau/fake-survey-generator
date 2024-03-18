using FakeSurveyGenerator.Application.Infrastructure.Persistence.Interceptors;
using FakeSurveyGenerator.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence;

internal static class DatabaseServiceCollectionExtensions
{
    internal static readonly string[] DbTags = ["fake-survey-generator-db", "ready"];

    public static IHostApplicationBuilder AddDatabaseConfiguration(this IHostApplicationBuilder builder,
        IConfiguration configuration)
    {
        const string connectionName = "database";

        var connectionString = configuration.GetConnectionString(connectionName) ??
                               throw new InvalidOperationException(
                                   $"Connection String for '{connectionName}' was not found in config");

        builder.Services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        builder.AddSqlServerDbContext<SurveyContext>(connectionName, settings =>
        {
            settings.DbContextPooling = false;
            settings.MaxRetryCount = 5;
        });

        //services.AddDbContext<SurveyContext>
        //(options =>
        //    {
        //        options.UseSqlServer(connectionString,
        //            sqlServerOptions =>
        //                sqlServerOptions.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30),
        //                    null));
        //    }
        //);

        builder.Services.AddScoped<IDatabaseConnection>(_ => new DapperSqlServerConnection(connectionString));

        //var healthChecksBuilder = services.AddHealthChecks();
        //healthChecksBuilder.AddDatabaseHealthCheck(configuration);

        return builder;
    }

    public static IHealthChecksBuilder AddDatabaseHealthCheck(this IHealthChecksBuilder healthChecksBuilder,
        IConfiguration configuration)
    {
        healthChecksBuilder
            .AddSqlServer(
                configuration.GetConnectionString(nameof(SurveyContext)) ?? throw new InvalidOperationException($"Connection String for {nameof(SurveyContext)} not found in configuration"),
                name: "FakeSurveyGeneratorDB-check",
                tags: DbTags,
                failureStatus: HealthStatus.Unhealthy);

        return healthChecksBuilder;
    }
}