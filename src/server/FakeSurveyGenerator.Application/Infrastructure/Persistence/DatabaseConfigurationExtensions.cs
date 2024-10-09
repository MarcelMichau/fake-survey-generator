using FakeSurveyGenerator.Application.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence;

internal static class DatabaseConfigurationExtensions
{
    public static IHostApplicationBuilder AddDatabaseConfiguration(this IHostApplicationBuilder builder,
        IConfiguration configuration)
    {
        const string connectionName = "database";

        var connectionString = configuration.GetConnectionString(connectionName) ??
                               throw new InvalidOperationException(
                                   $"Connection String for '{connectionName}' was not found in config");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntitySaveChangesInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<SurveyContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseSqlServer(connectionString);
            }
        );

        builder.EnrichSqlServerDbContext<SurveyContext>();

        return builder;
    }
}