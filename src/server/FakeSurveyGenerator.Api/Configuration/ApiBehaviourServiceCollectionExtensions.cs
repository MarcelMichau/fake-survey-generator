﻿using Microsoft.AspNetCore.Mvc;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class ApiBehaviourServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddApiBehaviourConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        return builder;
    }
}