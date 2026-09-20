using FakeSurveyGenerator.Application.Features.Surveys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.Application.Infrastructure.TypeSafe;

internal static class TypeSafeConfigurationExtensions
{
    public static IHostApplicationBuilder AddTypeSafeConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddHttpClient<ISurveySemanticAnalyzer, TypeSafeSurveySemanticAnalyzer>((serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var baseUrl = configuration["TYPESAFE_BASE_URL"] ?? "https://api.typesafe.ai/";
                if (!baseUrl.EndsWith('/'))
                {
                    baseUrl += "/";
                }

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = Timeout.InfiniteTimeSpan;
            });

        return builder;
    }
}
