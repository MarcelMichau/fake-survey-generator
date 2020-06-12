using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FakeSurveyGenerator.API.Builders
{
    internal static class AuthenticationBuilder
    {
        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            var identityProviderUrl = configuration.GetValue<string>("IDENTITY_PROVIDER_URL");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = identityProviderUrl;
                    options.Audience = "fake-survey-generator-api";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = identityProviderUrl
                    };
                });

            return services;
        }
    }
}
