using FakeSurveyGenerator.Application.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.Application.Infrastructure.Persistence;

internal static class DatabaseConfigurationExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddDatabaseConfiguration()
        {
            const string connectionName = "database";

            var connectionString = builder.Configuration.GetConnectionString(connectionName) ??
                                   throw new InvalidOperationException(
                                       $"Connection String for '{connectionName}' was not found in config");

            builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntitySaveChangesInterceptor>();
            builder.Services.AddScoped<ISaveChangesInterceptor, PublishDomainEventsInterceptor>();

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
}