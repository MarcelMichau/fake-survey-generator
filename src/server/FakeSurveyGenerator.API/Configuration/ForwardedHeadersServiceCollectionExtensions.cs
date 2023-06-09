using Microsoft.AspNetCore.HttpOverrides;

namespace FakeSurveyGenerator.API.Configuration;

internal static class ForwardedHeadersServiceCollectionExtensions
{
    public static IServiceCollection AddForwardedHeadersConfiguration(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;

            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

            options.AllowedHosts = new List<string>
            {
                "fakesurveygenerator.mysecondarydomain.com"
            };
        });

        return services;
    }
}