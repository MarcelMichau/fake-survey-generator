namespace FakeSurveyGenerator.Api.Admin;

internal static class AdminEndpoints
{
    internal static void MapAdminEndpoints(this IEndpointRouteBuilder app)
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

    private static IResult GetVersionInformation()
    {
        return Results.Ok(new
        {
            ThisAssembly.AssemblyVersion,
            ThisAssembly.AssemblyFileVersion,
            ThisAssembly.AssemblyInformationalVersion,
            ThisAssembly.AssemblyName,
            ThisAssembly.AssemblyTitle,
            ThisAssembly.AssemblyConfiguration,
            ThisAssembly.RootNamespace,
            ThisAssembly.GitCommitDate,
            ThisAssembly.GitCommitId
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