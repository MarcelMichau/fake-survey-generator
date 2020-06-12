using System;
using System.Net;
using System.Net.Http;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace FakeSurveyGenerator.Infrastructure.Builders
{
    internal static class HttpClientBuilder
    {
        public static IServiceCollection AddHttpClientConfiguration(this IServiceCollection services)
        {
            var commonResilience = Policy.WrapAsync(GetTimeoutPolicy(), GetRetryPolicy(), GetCircuitBreakerPolicy());
            
            services
                .AddHttpClient<IUserService, OAuthUserInfoService>()
                .AddPolicyHandler(commonResilience);

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(5);
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(delay);
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }
    }
}
