using System;
using FakeSurveyGenerator.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.Infrastructure.Persistence
{
    internal static class DatabaseServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(nameof(SurveyContext));

            services.AddScoped<IConnectionString>(sp => new ConnectionString(connectionString));

            services.AddDbContext<SurveyContext>
            (options =>
                {
                    options.UseSqlServer(connectionString,
                        sqlServerOptions =>
                            sqlServerOptions.EnableRetryOnFailure(15, TimeSpan.FromSeconds(30),
                                null));

                    if (configuration.GetValue<bool>("SQL_SERVER_USE_AZURE_AD_AUTHENTICATION"))
                    {
                        options.UseAzureAccessToken();
                    }
                }
            );

            services.AddScoped<ISurveyContext>(provider => provider.GetService<SurveyContext>());

            return services;
        }
    }
}