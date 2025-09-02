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
builder.Services.AddProblemDetails();

builder
    .AddServiceDefaults()
    .AddTelemetryConfiguration()
    .AddDaprConfiguration()
    .AddSwaggerConfiguration()
    .AddAuthenticationConfiguration()
    .AddForwardedHeadersConfiguration()
    .AddApiBehaviourConfiguration()
    .AddApplicationServicesConfiguration();

if (!builder.Configuration.GetValue<bool>("SKIP_DAPR"))
    builder.Configuration.AddDaprSecretStore("secrets", new DaprClientBuilder().Build(), TimeSpan.FromSeconds(10));

var app = builder.Build();

app.UseSecurityHeaders();
app.UseForwardedHeaders();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseHealthChecksConfiguration();

app.UseSwaggerConfiguration();

app.MapAdminEndpoints();
app.MapSurveyEndpoints();
app.MapUserEndpoints();

app.MapDefaultEndpoints();

app.Run();

public partial class Program;