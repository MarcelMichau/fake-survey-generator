using System;
using FakeSurveyGenerator.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace FakeSurveyGenerator.API.Configuration
{
    internal static class AuthenticationServiceCollectionExtensions
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

            services.AddScoped<ITokenProviderService>(sp => new JwtBearerTokenProviderService(sp));

            return services;
        }

        private class JwtBearerTokenProviderService : ITokenProviderService
        {
            private readonly IServiceProvider _serviceProvider;

            public JwtBearerTokenProviderService(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public string GetToken()
            {
                var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;

                if (httpContext.User.Identity != null && !httpContext.User.Identity.IsAuthenticated)
                    throw new InvalidOperationException("Cannot retrieve a token for an unauthorized user");

                var accessToken = httpContext.Request.Headers[HeaderNames.Authorization].ToString().Substring(7);

                return accessToken;
            }
        }
    }
}
