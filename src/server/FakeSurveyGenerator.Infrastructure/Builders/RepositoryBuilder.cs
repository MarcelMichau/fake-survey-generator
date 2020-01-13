using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.Infrastructure.Builders
{
    public static class RepositoryBuilder
    {
        public static IServiceCollection AddRepositoryConfiguration(this IServiceCollection services)
        {
            services.AddScoped<ISurveyRepository, SurveyRepository>();

            return services;
        }
    }
}
