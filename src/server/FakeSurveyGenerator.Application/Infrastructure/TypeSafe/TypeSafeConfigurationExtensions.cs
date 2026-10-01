using FakeSurveyGenerator.Application.Features.Surveys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;

namespace FakeSurveyGenerator.Application.Infrastructure.TypeSafe;

internal static class TypeSafeConfigurationExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddTypeSafeConfiguration()
        {
#pragma warning disable EXTEXP0001 // Replace the default handler rather than stacking resilience pipelines.
            var httpClient = builder.Services
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
            httpClient.RemoveAllResilienceHandlers()
                .AddStandardResilienceHandler(options =>
                {
                    // Analysis can take longer than a normal API call. Never repeat provider work.
                    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(60);
                    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
                    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(120);
                    options.RateLimiter.DefaultRateLimiterOptions.PermitLimit = 8;
                    options.RateLimiter.DefaultRateLimiterOptions.QueueLimit = 0;
                    builder.Configuration.GetSection("TypeSafe:Resilience").Bind(options);
                    options.Retry.DisableForUnsafeHttpMethods();
                });
            // The handler timeouts cover response headers. Keep a native end-to-end
            // timeout as well so a stalled response body cannot run indefinitely.
            httpClient.ConfigureHttpClient(client => client.Timeout = builder.Configuration
                .GetValue<TimeSpan?>("TypeSafe:Resilience:TotalRequestTimeout:Timeout") ?? TimeSpan.FromSeconds(60));
#pragma warning restore EXTEXP0001

            return builder;
        }
    }
}
