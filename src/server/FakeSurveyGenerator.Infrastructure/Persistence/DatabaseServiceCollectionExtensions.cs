using FakeSurveyGenerator.Application.Common.Persistence;
using FakeSurveyGenerator.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.Infrastructure.Persistence;

internal static class DatabaseServiceCollectionExtensions
{
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

        services.AddScoped<ISurveyContext>(provider =>
            provider.GetService<SurveyContext>() ??
            throw new InvalidOperationException($"{nameof(SurveyContext)} was not registered in IServiceProvider"));

        services.AddScoped<IDatabaseConnection>(_ => new DapperSqlServerConnection(connectionString));

        return services;
    }
}