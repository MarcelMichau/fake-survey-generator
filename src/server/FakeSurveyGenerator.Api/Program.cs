using Dapr.Client;
using Dapr.Extensions.Configuration;
using FakeSurveyGenerator.Api.Admin;
using FakeSurveyGenerator.Api.Configuration;
using FakeSurveyGenerator.Api.Configuration.HealthChecks;
using FakeSurveyGenerator.Api.Configuration.Swagger;
using FakeSurveyGenerator.Api.Surveys;
using FakeSurveyGenerator.Api.Users;

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

if (!builder.Configuration.GetValue<bool>("SKIP_DAPR"))
{
    var configStoreName =
        builder.Environment.IsDevelopment() ? "local-file" : "azure-key-vault";

    var daprClient = new DaprClientBuilder().Build();

    builder.Configuration.AddDaprSecretStore(configStoreName, daprClient, TimeSpan.FromSeconds(10));
}

var app = builder.Build();

app.UseSecurityHeaders();
app.UseForwardedHeaders();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
    app.UseCors(); // CORS is only used for local development when running in the Aspire Host

app.UseHttpsRedirection();

app.UseHealthChecksConfiguration();

app.UseSwaggerConfiguration();

app.MapAdminEndpoints();
app.MapSurveyEndpoints();
app.MapUserEndpoints();

app.MapDefaultEndpoints();

app.Run();

public partial class Program;
