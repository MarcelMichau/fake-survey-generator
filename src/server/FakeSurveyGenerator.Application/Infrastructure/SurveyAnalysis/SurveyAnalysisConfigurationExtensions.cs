using Azure.Identity;
using FakeSurveyGenerator.Application.Features.Surveys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

namespace FakeSurveyGenerator.Application.Infrastructure.SurveyAnalysis;

internal static class SurveyAnalysisConfigurationExtensions
{
    private const string ResilienceSectionName = $"{SurveyAnalysisOptions.SectionName}:Resilience";

    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddSurveyAnalysisConfiguration()
        {
            var surveyAnalysisSection = builder.Configuration.GetSection(SurveyAnalysisOptions.SectionName);
            builder.Services.Configure<SurveyAnalysisOptions>(surveyAnalysisSection);

            // Microsoft Foundry only accepts Microsoft Entra tokens: a managed identity when deployed,
            // or a developer credential locally.
            builder.Services.AddSingleton<ISurveyAnalysisTokenProvider>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<SurveyAnalysisOptions>>().Value;
                var credentialOptions = new DefaultAzureCredentialOptions();
                if (!string.IsNullOrWhiteSpace(options.TenantId))
                {
                    credentialOptions.TenantId = options.TenantId;
                }

                return new AzureCredentialSurveyAnalysisTokenProvider(
                    new DefaultAzureCredential(credentialOptions),
                    serviceProvider.GetService<TimeProvider>() ?? TimeProvider.System);
            });

#pragma warning disable EXTEXP0001 // Replace the default handler rather than stacking resilience pipelines.
            var httpClient = builder.Services
                .AddHttpClient<ISurveySemanticAnalyzer, DecisionSurveySemanticAnalyzer>((serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<SurveyAnalysisOptions>>().Value;
                    if (!string.IsNullOrWhiteSpace(options.Endpoint))
                    {
                        client.BaseAddress = new Uri($"{options.Endpoint.TrimEnd('/')}/providers/microsoft/");
                    }

                    client.Timeout = Timeout.InfiniteTimeSpan;
                });
            httpClient.RemoveAllResilienceHandlers()
                .AddStandardResilienceHandler(options =>
                {
                    // Analysis can take longer than a normal API call. Never repeat provider work.
                    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(55);
                    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
                    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(120);
                    options.RateLimiter.DefaultRateLimiterOptions.PermitLimit = 8;
                    options.RateLimiter.DefaultRateLimiterOptions.QueueLimit = 0;
                    builder.Configuration.GetSection(ResilienceSectionName).Bind(options);
                    if (options.AttemptTimeout.Timeout >= options.TotalRequestTimeout.Timeout)
                    {
                        throw new OptionsValidationException(ResilienceSectionName, typeof(HttpStandardResilienceOptions),
                            ["The survey analysis attempt timeout must be shorter than the total request timeout."]);
                    }

                    options.Retry.DisableForUnsafeHttpMethods();
                });
            // Buffer inside the resilience handler so its timeouts, circuit breaker and
            // concurrency permits cover the body too. No native HttpClient timeout is needed.
            httpClient.AddHttpMessageHandler(() => new SurveyAnalysisResponseBufferingHandler());
#pragma warning restore EXTEXP0001

            return builder;
        }
    }
}
