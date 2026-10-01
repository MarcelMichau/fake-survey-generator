using FakeSurveyGenerator.Api.Admin;
using FakeSurveyGenerator.Api.Configuration;
using FakeSurveyGenerator.Api.Configuration.HealthChecks;
using FakeSurveyGenerator.Api.Configuration.OpenApi;
using FakeSurveyGenerator.Api.Configuration.SecurityHeaders;
using FakeSurveyGenerator.Api.Surveys;
using FakeSurveyGenerator.Api.Users;
using FakeSurveyGenerator.Application;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();

builder.AddOpenApiConfiguration();

// The build-time generator needs endpoint registrations, but no live infrastructure.
var generatingOpenApi = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
if (generatingOpenApi)
{
    builder.AddApplication();
    builder.Host.UseDefaultServiceProvider(options => options.ValidateOnBuild = false);
}
else
{
    builder
        .AddServiceDefaults()
        .AddDaprConfiguration()
        .AddAuthenticationConfiguration()
        .AddForwardedHeadersConfiguration()
        .AddApplicationServicesConfiguration();
}

var app = builder.Build();

if (!generatingOpenApi)
{
    app.UseSecurityHeadersConfiguration();

    app.UseFileServer();

    app.UseHttpsRedirection();

    app.UseHealthChecksConfiguration();

    app.UseOpenApiConfiguration();
    app.MapDefaultEndpoints();
}

app.MapAdminEndpoints();
app.MapSurveyEndpoints();
app.MapUserEndpoints();

app.Run();
