using Microsoft.AspNetCore.Mvc;

namespace FakeSurveyGenerator.API.Configuration;

internal static class ApiBehaviourServiceCollectionExtensions
{
    public static IServiceCollection AddApiBehaviourConfiguration(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        return services;
    }
}