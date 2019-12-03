using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FakeSurveyGenerator.API.Builders
{
    public static class AuthenticationBuilder
    {
        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = configuration.GetValue<string>("IDENTITY_PROVIDER_BACKCHANNEL_URL");
                    options.RequireHttpsMetadata = false;
                    options.Audience = "fake-survey-generator-api";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuers = new List<string>
                        {
                            configuration.GetValue<string>("IDENTITY_PROVIDER_FRONTCHANNEL_URL"),
                            configuration.GetValue<string>("IDENTITY_PROVIDER_BACKCHANNEL_URL")
                        }
                    };
                });

            return services;
        }
    }
}
