using FakeSurveyGenerator.Application.Shared.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace FakeSurveyGenerator.Application.Infrastructure.Identity;

internal static class IdentityProviderConfigurationExtensions
{
    public static IServiceCollection AddOAuthConfiguration(this IServiceCollection services)
    {
        services
            .AddHttpClient<IUserService, OAuthUserInfoService>()
            .AddStandardResilienceHandler();

        services.AddHttpContextAccessor();
        services.AddScoped<ITokenProviderService, JwtBearerTokenProviderService>();

        return services;
    }
}