using AutoWrapper;
using FakeSurveyGenerator.API.Configuration;
using FakeSurveyGenerator.API.Configuration.HealthChecks;
using FakeSurveyGenerator.API.Configuration.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Serilog;

namespace FakeSurveyGenerator.API
{
    public sealed class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization()
                .AddHealthChecksConfiguration(_configuration)
                .AddSwaggerConfiguration(_configuration)
                .AddAuthenticationConfiguration(_configuration)
                .AddForwardedHeadersConfiguration()
                .AddApplicationInsightsConfiguration(_configuration)
                .AddApplicationServicesConfiguration(_configuration)
                .AddApiBehaviourConfiguration()
                .AddControllers()
                    .AddJsonConfiguration()
                    .AddValidationConfiguration()
                    .AddExceptionHandlingConfiguration();
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSecurityHeaders();

            app.UseForwardedHeaders();

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

            app.UseSerilogRequestLogging();

            app.UseApiResponseAndExceptionWrapper();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.UseHealthChecksConfiguration();
            });

            app.UseSwaggerConfiguration();
        }
    }
}