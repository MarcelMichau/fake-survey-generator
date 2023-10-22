using FakeSurveyGenerator.Application.Infrastructure.Persistence.Interceptors;
using FakeSurveyGenerator.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence;

internal static class DatabaseServiceCollectionExtensions
{
    internal static readonly string[] DbTags = { "fake-survey-generator-db", "ready" };

    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(nameof(SurveyContext)) ??
                               throw new InvalidOperationException(
                                   $"Connection String for {nameof(SurveyContext)} was not found in config");

        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<SurveyContext>
        (options =>
            {
                options.UseSqlServer(connectionString,
                    sqlServerOptions =>
                        sqlServerOptions.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30),
                            null));
            }
        );

        services.AddScoped<IDatabaseConnection>(_ => new DapperSqlServerConnection(connectionString));

        var healthChecksBuilder = services.AddHealthChecks();
        healthChecksBuilder.AddDatabaseHealthCheck(configuration);

        return services;
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