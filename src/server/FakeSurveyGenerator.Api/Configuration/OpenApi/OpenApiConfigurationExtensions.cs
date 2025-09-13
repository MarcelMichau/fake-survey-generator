using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Net;

namespace FakeSurveyGenerator.Api.Configuration.OpenApi;
internal static class OpenApiConfigurationExtensions
{
    public static IHostApplicationBuilder AddOpenApiConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<OAuth2SecuritySchemeTransformer>();
            options.AddOperationTransformer<AuthorizeOperationTransformer>();
        });

        return builder;
    }

    public static IApplicationBuilder UseOpenApiConfiguration(this WebApplication app)
    {
        app.MapOpenApi();

        app.MapScalarApiReference("/api-docs", options =>
        {
            options.Title = "Fake Survey Generator - OpenAPI";

            // Because light attracts bugs :)
            options.DarkMode = true;
            options.HideDarkModeToggle = true;

            // Use the Aspire external proxy address for the API instead of the internal API address for the URL used by Scalar
            // https://github.com/scalar/scalar/discussions/4025
            options.Servers = [];

            options
                .AddPreferredSecuritySchemes("OAuth2")
                .AddAuthorizationCodeFlow("OAuth2", flow =>
                {
                    flow.ClientId = "LuAbezRfaAKRau0myoAkXCK2myLrfMYP";
                    flow.Pkce = Pkce.Sha256;
                    flow.SelectedScopes = ["openid", "profile", "email"];
                })
                .WithPersistentAuthentication();
        });

        return app;
    }
}

internal sealed class OAuth2SecuritySchemeTransformer(IConfiguration configuration) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "Fake Survey Generator API",
            Version = ThisAssembly.AssemblyFileVersion,
            Description = "This is an API. That generates surveys. Fake ones. For fun. That is all.",
            License = new OpenApiLicense
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/licenses/MIT")
            },
            Contact = new OpenApiContact
            {
                Name = "Marcel Michau",
                Email = string.Empty,
                Url = new Uri("https://marcelmichau.dev")
            }
        };

        var identityProviderBaseUrl = configuration.GetValue<string>("IDENTITY_PROVIDER_URL")?.TrimEnd('/');

        if (string.IsNullOrEmpty(identityProviderBaseUrl)) return Task.CompletedTask;

        var scheme = new OpenApiSecurityScheme
        {
            Description = "OpenID Connect Authentication",
            OpenIdConnectUrl = new Uri($"{identityProviderBaseUrl}/.well-known/openid-configuration"),
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl =
                        new Uri($"{identityProviderBaseUrl}/authorize?audience=fake-survey-generator-api"),
                    TokenUrl = new Uri($"{identityProviderBaseUrl}/oauth/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "Standard OpenID Scope" },
                        { "profile", "Standard OpenID Scope" },
                        { "email", "Standard OpenID Scope" }
                    }
                }
            }
        };

        const string referenceId = "OAuth2";
        var reference = new OpenApiSecuritySchemeReference(referenceId, document);

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[referenceId] = scheme;
        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement { [reference] = [] });

        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations ?? []))
            operation.Value.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    [reference] = ["openid", "profile", "email"]
                }
            };

        return Task.CompletedTask;
    }
}

internal sealed class AuthorizeOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        // Inspect endpoint metadata (works for controllers and minimal APIs)
        var metadata = context.Description?.ActionDescriptor?.EndpointMetadata;

        if (metadata == null) return Task.CompletedTask;

        // If [AllowAnonymous] present, skip
        if (metadata.OfType<IAllowAnonymous>().Any()) return Task.CompletedTask;

        // If any [Authorize] metadata is present, mark operation as secured and add 401 response
        if (!metadata.OfType<IAuthorizeData>().Any()) return Task.CompletedTask;

        operation.Responses ??= new OpenApiResponses();
        operation.Responses[StatusCodes.Status401Unauthorized.ToString()] =
            new OpenApiResponse { Description = nameof(HttpStatusCode.Unauthorized) };

        // Reference the OAuth2 security scheme defined at document level
        const string referenceId = "OAuth2";
        var reference = new OpenApiSecuritySchemeReference(referenceId, context.Document);

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [reference] = ["openid", "profile", "email"]
        });

        return Task.CompletedTask;
    }
}