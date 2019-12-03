using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
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
                c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "OAuth2 Authentication",
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

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            return app.UseSwaggerUI(c =>
            {
                c.EnableDeepLinking();
                c.InjectJavascript("/swagger/idTokenOverride.js");
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fake Survey Generator API V1");
                c.OAuthClientId("fake-survey-generator-api-swagger");
            });
        }
    }
}
