using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace FakeSurveyGenerator.API.Builders
{
    public static class SwaggerBuilder
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Fake Survey Generator API",
                    Version = "v1",
                    Description = "This is an API. That generates surveys. Fake ones. For fun. That is all.",
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

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "OpenID Connect Authentication",
                    OpenIdConnectUrl = new Uri($"{configuration.GetValue<string>("IDENTITY_PROVIDER_FRONTCHANNEL_URL")}/.well-known/openid-configuration"),
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{configuration.GetValue<string>("IDENTITY_PROVIDER_FRONTCHANNEL_URL")}/connect/authorize"),
                            Scopes = new Dictionary<string, string> { { "openid", "Standard OpenID Scope" }, { "profile", "Standard OpenID Scope" }, { "fake-survey-generator-api", "Grants access to the Fake Survey Generator API" } }
                        }
                    }
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "oauth2"}
                        },
                        new List<string>{ "openid", "profile", "fake-survey-generator-api" }
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
                options.InjectJavascript("/swagger/idTokenOverride.js");
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Fake Survey Generator API V1");
                options.OAuthClientId("fake-survey-generator-api-swagger");
            });
        }
    }
}
