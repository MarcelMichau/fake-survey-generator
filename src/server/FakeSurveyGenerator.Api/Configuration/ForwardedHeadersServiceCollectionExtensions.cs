﻿using Microsoft.AspNetCore.HttpOverrides;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class ForwardedHeadersServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddForwardedHeadersConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;

            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

            options.AllowedHosts = new List<string>
            {
                "fakesurveygenerator.mysecondarydomain.com"
            };
        });

        return builder;
    }
}