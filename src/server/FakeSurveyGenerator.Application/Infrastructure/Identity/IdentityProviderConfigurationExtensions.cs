using FakeSurveyGenerator.Application.Shared.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace FakeSurveyGenerator.Application.Infrastructure.Identity;

internal static class IdentityProviderConfigurationExtensions
{
    private static readonly string[] IdentityProviderTags = ["identity-provider", "ready"];

    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddOAuthConfiguration()
        {
            builder.Services
                .AddHttpClient<IUserService, OAuthUserInfoService>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ITokenProviderService, JwtBearerTokenProviderService>();

            var healthChecksBuilder = builder.Services.AddHealthChecks();
            healthChecksBuilder.AddIdentityProviderHealthCheck(builder.Configuration);

            return builder;
        }
    }

    extension(IHealthChecksBuilder healthChecksBuilder)
    {
        private IHealthChecksBuilder AddIdentityProviderHealthCheck(IConfiguration configuration)
        {
            healthChecksBuilder.AddOpenIdConnectServer(
                new Uri($"{configuration.GetValue<string>("IDENTITY_PROVIDER_URL")}"),
                name: "IdentityProvider-check",
                tags: IdentityProviderTags,
                failureStatus: HealthStatus.Unhealthy,
                timeout: new TimeSpan(0, 0, 5));

            return healthChecksBuilder;
        }
    }
}
