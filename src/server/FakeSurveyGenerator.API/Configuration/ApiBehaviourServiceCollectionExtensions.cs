using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Configuration
{
    internal static class ApiBehaviourServiceCollectionExtensions
    {
        public static IServiceCollection AddApiBehaviourConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            return services;
        }
    }
}
