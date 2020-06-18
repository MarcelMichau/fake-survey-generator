using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace FakeSurveyGenerator.API.Builders.Swagger
{
    internal static class SwaggerBuilder
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Fake Survey Generator API",
                    Version = ThisAssembly.AssemblyInformationalVersion,
                    Description = "This is an API. That generates surveys. Fake ones. For fun. That is all.",
                    License = new OpenApiLicense
                    {
                        Name = "MIT",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    },
                    Contact = new OpenApiContact
                    {
                        Name = "Marcel Michau",
                        Email = string.Empty,
                        Url = new Uri("https://marcelmichau.dev")
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                options.OperationFilter<AuthorizeOperationFilter>();

                var identityProviderBaseUrl = configuration.GetValue<string>("IDENTITY_PROVIDER_URL").TrimEnd('/');

                options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
                {
                    Description = "OpenID Connect Authentication",
                    OpenIdConnectUrl = new Uri($"{identityProviderBaseUrl}/.well-known/openid-configuration"),
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl =
                                new Uri($"{identityProviderBaseUrl}/authorize?audience=fake-survey-generator-api"),
                            TokenUrl = new Uri($"{identityProviderBaseUrl}/oauth/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                {"openid", "Standard OpenID Scope"}, {"profile", "Standard OpenID Scope"},
                                {"email", "Standard OpenID Scope"}
                            }
                        }
                    }
                });
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
        {
            app.UseSwagger();

            return app.UseSwaggerUI(options =>
            {
                options.DocumentTitle = "Fake Survey Generator - Swagger";
                options.EnableDeepLinking();
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Fake Survey Generator API V1");
                options.OAuthAppName("Fake Survey Generator Swagger");
                options.OAuthClientId("LuAbezRfaAKRau0myoAkXCK2myLrfMYP");
                options.OAuthUsePkce();
                options.DisplayRequestDuration();
            });
        }
    }
}