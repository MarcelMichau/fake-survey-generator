using FakeSurveyGenerator.Api.Admin;
using FakeSurveyGenerator.Api.Configuration;
using FakeSurveyGenerator.Api.Configuration.HealthChecks;
using FakeSurveyGenerator.Api.Configuration.OpenApi;
using FakeSurveyGenerator.Api.Configuration.SecurityHeaders;
using FakeSurveyGenerator.Api.Surveys;
using FakeSurveyGenerator.Api.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();

builder
    .AddServiceDefaults()
    .AddDaprConfiguration()
    .AddOpenApiConfiguration()
    .AddAuthenticationConfiguration()
    .AddApplicationServicesConfiguration();

var app = builder.Build();

app.UseSecurityHeadersConfiguration();

app.UseForwardedHeaders();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseHealthChecksConfiguration();

app.UseOpenApiConfiguration();

app.MapAdminEndpoints();
app.MapSurveyEndpoints();
app.MapUserEndpoints();

app.MapDefaultEndpoints();

app.Run();