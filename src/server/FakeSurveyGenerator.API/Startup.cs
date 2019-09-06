using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AutoMapper;
using FakeSurveyGenerator.API.Application.Queries;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Infrastructure;
using FakeSurveyGenerator.Infrastructure.Repositories;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

namespace FakeSurveyGenerator.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization();

            services.AddControllers()
                .AddNewtonsoftJson();

            var currentAssembly = typeof(Startup).GetTypeInfo().Assembly;

            services.AddAutoMapper(currentAssembly);
            services.AddMediatR(currentAssembly);

            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = new ConfigurationOptions
                {
                    EndPoints = {_configuration.GetValue<string>("REDIS_URL")},
                    Password = _configuration.GetValue<string>("REDIS_PASSWORD"),
                    Ssl = _configuration.GetValue<bool>("REDIS_SSL"),
                    DefaultDatabase = _configuration.GetValue<int>("REDIS_DEFAULT_DATABASE")
                };
            });

            var connectionString = _configuration.GetConnectionString(nameof(SurveyContext));

            services.AddDbContext<SurveyContext>
            (options => options.UseSqlServer(connectionString,
                b => b.MigrationsAssembly("FakeSurveyGenerator.Infrastructure")));

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
                    OpenIdConnectUrl = new Uri("https://localhost:44320/.well-known/openid-configuration"),
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("https://localhost:44320/connect/authorize"),
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

            services.AddCustomHealthChecks(_configuration);

            SetupDi(services, connectionString);

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = _configuration.GetValue<string>("IDENTITY_PROVIDER_URL");
                    options.RequireHttpsMetadata = false;
                    options.Audience = "fake-survey-generator-api";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuers = new List<string> {"https://localhost:44320", _configuration.GetValue<string>("IDENTITY_PROVIDER_URL") }
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true;
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });

            app.UseHealthChecksUI();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.EnableDeepLinking();
                c.InjectJavascript("/swagger/idTokenOverride.js");
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fake Survey Generator API V1");
                c.OAuthClientId("fake-survey-generator-api-swagger");
            });

            UpdateDatabase(app, env);
        }

        private static void SetupDi(IServiceCollection services, string connectionString)
        {
            services.AddScoped<ISurveyQueries>(serviceProvider => new SurveyQueries(connectionString,
                serviceProvider.GetService<IDistributedCache>()));
            services.AddScoped<ISurveyRepository, SurveyRepository>();
        }

        private static void UpdateDatabase(IApplicationBuilder app, IHostEnvironment env)
        {
            if (!env.IsDevelopment()) // Only migrate database on startup when running in Development environment
                return;

            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            using var context = serviceScope.ServiceProvider.GetService<SurveyContext>();
            if (context.Database.IsSqlServer()) // Do not migrate database when running integration tests with in-memory database
                context.Database.Migrate();
        }
    }
}