using Microsoft.AspNetCore.HttpOverrides;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class ForwardedHeadersConfigurationExtensions
{
    public static IHostApplicationBuilder AddForwardedHeadersConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders |= ForwardedHeaders.XForwardedHost;

            options.AllowedHosts =
            [
                "fakesurveygenerator.mysecondarydomain.com"
            ];
        });

        return builder;
    }
}