using FakeSurveyGenerator.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Builders
{
    public static class DatabaseBuilder
    {
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(nameof(SurveyContext));

            services.AddDbContext<SurveyContext>
            (options => options.UseSqlServer(connectionString,
                b => b.MigrationsAssembly(typeof(SurveyContext).Namespace)));

            return services;
        }
    }
}
