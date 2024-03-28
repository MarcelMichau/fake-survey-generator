using Microsoft.IdentityModel.Tokens;

namespace FakeSurveyGenerator.Api.Configuration;

internal static class AuthenticationConfigurationExtensions
{
    public static IHostApplicationBuilder AddAuthenticationConfiguration(this IHostApplicationBuilder builder)
    {
        var identityProviderUrl = builder.Configuration.GetValue<string>("IDENTITY_PROVIDER_URL");

        builder.Services.AddAuthentication()
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

        return builder;
    }
}