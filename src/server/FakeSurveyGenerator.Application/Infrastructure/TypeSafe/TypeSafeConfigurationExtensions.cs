using Azure.Identity;
using FakeSurveyGenerator.Application.Features.Surveys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;

namespace FakeSurveyGenerator.Application.Infrastructure.TypeSafe;

internal static class TypeSafeConfigurationExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddTypeSafeConfiguration()
        {
            builder.AddTypeSafeAuthorization();

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
                    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(55);
                    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
                    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(120);
                    options.RateLimiter.DefaultRateLimiterOptions.PermitLimit = 8;
                    options.RateLimiter.DefaultRateLimiterOptions.QueueLimit = 0;
                    builder.Configuration.GetSection("TypeSafe:Resilience").Bind(options);
                    if (options.AttemptTimeout.Timeout >= options.TotalRequestTimeout.Timeout)
                    {
                        throw new OptionsValidationException("TypeSafe:Resilience", typeof(HttpStandardResilienceOptions),
                            ["The TypeSafe attempt timeout must be shorter than the total request timeout."]);
                    }

                    options.Retry.DisableForUnsafeHttpMethods();
                });
            // Buffer inside the resilience handler so its timeouts, circuit breaker and
            // concurrency permits cover the body too. No native HttpClient timeout is needed.
            httpClient.AddHttpMessageHandler(() => new TypeSafeResponseBufferingHandler());
#pragma warning restore EXTEXP0001

            return builder;
        }

        private void AddTypeSafeAuthorization()
        {
            var authMode = builder.Configuration["TYPESAFE_AUTH_MODE"];
            if (string.IsNullOrWhiteSpace(authMode) || authMode.Equals("ApiKey", StringComparison.OrdinalIgnoreCase))
            {
                builder.Services.AddSingleton<ITypeSafeAuthorizationProvider, ApiKeyTypeSafeAuthorizationProvider>();
            }
            else if (authMode.Equals("AzureCredential", StringComparison.OrdinalIgnoreCase))
            {
                // Microsoft Foundry deployments (e.g. Microsoft-Decision-1) accept Microsoft Entra tokens.
                // Developer tools can be signed in to several tenants, so allow pinning the resource's tenant.
                var credentialOptions = new DefaultAzureCredentialOptions();
                if (builder.Configuration["TYPESAFE_TENANT_ID"] is { Length: > 0 } tenantId)
                {
                    credentialOptions.TenantId = tenantId;
                }

                builder.Services.AddSingleton<ITypeSafeAuthorizationProvider>(serviceProvider =>
                    new AzureCredentialTypeSafeAuthorizationProvider(
                        new DefaultAzureCredential(credentialOptions),
                        serviceProvider.GetService<TimeProvider>() ?? TimeProvider.System));
            }
            else
            {
                throw new OptionsValidationException("TYPESAFE_AUTH_MODE", typeof(string),
                    [$"Unsupported TYPESAFE_AUTH_MODE '{authMode}'. Use 'ApiKey' or 'AzureCredential'."]);
            }
        }
    }
}
