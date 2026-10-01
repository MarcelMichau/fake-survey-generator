namespace FakeSurveyGenerator.Api.Admin;

internal static class AdminEndpoints
{
    extension(IEndpointRouteBuilder app)
    {
        internal void MapAdminEndpoints()
        {
            var adminGroup = app.MapGroup("/api/admin");

            adminGroup.MapGet("/version", GetVersionInformation)
                .WithName(nameof(GetVersionInformation))
                .WithSummary("Returns API version information");

            adminGroup.MapGet("/ping", Ping)
                .WithName(nameof(Ping))
                .WithSummary("Returns a 200 OK Result. Used for testing network latency and as a sanity check");

            adminGroup.MapGet("/secret-test", SecretTest)
                .WithName(nameof(SecretTest))
                .WithSummary(
                    "Retrieves a test secret from Dapr Secret Store. Uses local file in Development & Azure Key Vault in Production");
        }
    }

    private static Microsoft.AspNetCore.Http.HttpResults.Ok<ApiVersionModel> GetVersionInformation()
    {
        return TypedResults.Ok(new ApiVersionModel
        {
            AssemblyVersion = ThisAssembly.AssemblyVersion,
            AssemblyFileVersion = ThisAssembly.AssemblyFileVersion,
            AssemblyInformationalVersion = ThisAssembly.AssemblyInformationalVersion,
            AssemblyName = ThisAssembly.AssemblyName,
            AssemblyTitle = ThisAssembly.AssemblyTitle,
            AssemblyConfiguration = ThisAssembly.AssemblyConfiguration,
            RootNamespace = ThisAssembly.RootNamespace,
            GitCommitDate = ThisAssembly.GitCommitDate,
            GitCommitId = ThisAssembly.GitCommitId
        });
    }

    private static IResult Ping()
    {
        return Results.Ok();
    }

    private static IResult SecretTest(IConfiguration configuration)
    {
        var secretValue = configuration.GetValue<string>("HealthCheckSecret");

        return Results.Ok(new
        {
            secretValue
        });
    }
}

internal sealed record ApiVersionModel
{
    public required string AssemblyVersion { get; init; }
    public required string AssemblyFileVersion { get; init; }
    public required string AssemblyInformationalVersion { get; init; }
    public required string AssemblyName { get; init; }
    public required string AssemblyTitle { get; init; }
    public required string AssemblyConfiguration { get; init; }
    public required string RootNamespace { get; init; }
    public required DateTime GitCommitDate { get; init; }
    public required string GitCommitId { get; init; }
}
