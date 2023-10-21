using Microsoft.IdentityModel.Tokens;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        var identityProviderUrl = configuration.GetValue<string>("IDENTITY_PROVIDER_URL");

        services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.Authority = identityProviderUrl;
                options.Audience = "fake-survey-generator-api";

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = identityProviderUrl,
                    ClockSkew = TimeSpan.FromSeconds(10)
                };
            });

        return services;
    }
}