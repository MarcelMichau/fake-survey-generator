using FakeSurveyGenerator.API.Application.Queries;
using FakeSurveyGenerator.API.HostedServices;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Infrastructure;
using FakeSurveyGenerator.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Builders
{
    public static class ApplicationServicesBuilder
    {
        public static IServiceCollection AddApplicationServicesConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(nameof(SurveyContext));

            services.AddScoped<ISurveyQueries>(serviceProvider => new SurveyQueries(connectionString,
                serviceProvider.GetService<IDistributedCache>()));
            services.AddScoped<ISurveyRepository, SurveyRepository>();

            services.AddHostedService<DatabaseCreationHostedService>();

            return services;
        }
    }
}
