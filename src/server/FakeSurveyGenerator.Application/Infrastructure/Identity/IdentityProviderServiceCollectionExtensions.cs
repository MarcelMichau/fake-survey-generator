using FakeSurveyGenerator.Application.Shared.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Http.Resilience;

namespace FakeSurveyGenerator.Application.Infrastructure.Identity;

internal static class IdentityProviderServiceCollectionExtensions
{
    internal static readonly string[] IdentityProviderTags = { "identity-provider", "ready" };

    public static IServiceCollection AddOAuthConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHttpClient<IUserService, OAuthUserInfoService>()
            .AddStandardResilienceHandler();

        services.AddHttpContextAccessor();
        services.AddScoped<ITokenProviderService, JwtBearerTokenProviderService>();

        var healthChecksBuilder = services.AddHealthChecks();
        healthChecksBuilder.AddIdentityProviderHealthCheck(configuration);

        return services;
    }

    internal static IHealthChecksBuilder AddIdentityProviderHealthCheck(this IHealthChecksBuilder healthChecksBuilder,
        IConfiguration configuration)
    {
        healthChecksBuilder.AddIdentityServer(
            new Uri($"{configuration.GetValue<string>("IDENTITY_PROVIDER_URL")}"),
            name: "IdentityProvider-check",
            tags: IdentityProviderTags,
            failureStatus: HealthStatus.Unhealthy,
            timeout: new TimeSpan(0, 0, 5));

        return healthChecksBuilder;
    }
}