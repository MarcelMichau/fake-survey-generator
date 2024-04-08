using Dapr.Client;
using Dapr.Extensions.Configuration;
using FakeSurveyGenerator.Api.Admin;
using FakeSurveyGenerator.Api.Configuration;
using FakeSurveyGenerator.Api.Configuration.HealthChecks;
using FakeSurveyGenerator.Api.Configuration.Swagger;
using FakeSurveyGenerator.Api.Surveys;
using FakeSurveyGenerator.Api.Users;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddAuthorization();

    builder
        .AddServiceDefaults()
        .AddTelemetryConfiguration()
        .AddCorsConfiguration()
        .AddDaprConfiguration()
        .AddSwaggerConfiguration()
        .AddAuthenticationConfiguration()
        .AddForwardedHeadersConfiguration()
        .AddApiBehaviourConfiguration()
        .AddApplicationServicesConfiguration();

    builder.Host
        .UseSerilog((hostBuilderContext, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console();
        }).ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
        {
            if (hostBuilderContext.Configuration.GetValue<bool>("SKIP_DAPR"))
                return;

            var configStoreName =
                hostBuilderContext.HostingEnvironment.IsDevelopment() ? "local-file" : "azure-key-vault";

            var daprClient = new DaprClientBuilder().Build();
            configurationBuilder.AddDaprSecretStore(configStoreName, daprClient, TimeSpan.FromSeconds(10));
        });

    var app = builder.Build();

    app.UseSecurityHeaders();
    app.UseForwardedHeaders();

    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseCors();

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    app.UseHealthChecksConfiguration();

    app.UseSwaggerConfiguration();

    app.MapAdminEndpoints();
    app.MapSurveyEndpoints();
    app.MapUserEndpoints();

    app.MapDefaultEndpoints();

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

public partial class Program;
