using Microsoft.AspNetCore.HttpOverrides;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class ForwardedHeadersConfigurationExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddForwardedHeadersConfiguration()
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
}