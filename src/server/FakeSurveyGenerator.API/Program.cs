using System;
using AutoWrapper;
using FakeSurveyGenerator.API.Configuration;
using FakeSurveyGenerator.API.Configuration.HealthChecks;
using FakeSurveyGenerator.API.Configuration.Swagger;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web host");

    var builder = WebApplication.CreateBuilder(args);
    
    builder.Host
        .UseSerilog((hostBuilderContext, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            if (!string.IsNullOrWhiteSpace(
                    hostBuilderContext.Configuration.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY")))
            {
                loggerConfiguration.WriteTo.ApplicationInsights(
                    services.GetRequiredService<TelemetryConfiguration>(),
                    TelemetryConverter.Traces);
            }
        });

    builder.WebHost
        .ConfigureKestrel(options => { options.AddServerHeader = false; });

    builder.Services
            .AddAuthorization()
            .AddHealthChecksConfiguration(builder.Configuration)
            .AddSwaggerConfiguration(builder.Configuration)
            .AddAuthenticationConfiguration(builder.Configuration)
            .AddForwardedHeadersConfiguration()
            .AddApplicationInsightsConfiguration(builder.Configuration)
            .AddApplicationServicesConfiguration(builder.Configuration)
            .AddApiBehaviourConfiguration()
            .AddControllers()
            .AddJsonConfiguration()
            .AddValidationConfiguration()
            .AddExceptionHandlingConfiguration();

    var app = builder.Build();

    app.UseSecurityHeaders();
    app.UseForwardedHeaders();

    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseSerilogRequestLogging();

    app.UseAutoWrapper();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.UseHealthChecksConfiguration();

    app.UseSwaggerConfiguration();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}