using Dapr.Client;
using Dapr.Extensions.Configuration;
using FakeSurveyGenerator.Api.Admin;
using FakeSurveyGenerator.Api.Configuration;
using FakeSurveyGenerator.Api.Configuration.HealthChecks;
using FakeSurveyGenerator.Api.Configuration.OpenApi;
using FakeSurveyGenerator.Api.Surveys;
using FakeSurveyGenerator.Api.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();

builder
    .AddServiceDefaults()
    .AddTelemetryConfiguration()
    .AddDaprConfiguration()
    .AddOpenApiConfiguration()
    .AddAuthenticationConfiguration()
    .AddForwardedHeadersConfiguration()
    .AddApiBehaviourConfiguration()
    .AddApplicationServicesConfiguration();

if (!builder.Configuration.GetValue<bool>("SKIP_DAPR"))
    builder.Configuration.AddDaprSecretStore("secrets", new DaprClientBuilder().Build(), TimeSpan.FromSeconds(10));

var app = builder.Build();

var policyCollection = new HeaderPolicyCollection()
     .AddCrossOriginOpenerPolicy(x => x.UnsafeNone()); // Required for OpenAPI Docs Sign-In with PKCE to work in browsers

app.UseSecurityHeaders(policyCollection);

app.UseForwardedHeaders();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseHealthChecksConfiguration();

app.UseSwaggerConfiguration();

app.MapAdminEndpoints();
app.MapSurveyEndpoints();
app.MapUserEndpoints();

app.MapDefaultEndpoints();

app.Run();