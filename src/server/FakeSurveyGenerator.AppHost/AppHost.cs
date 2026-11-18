using CommunityToolkit.Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddSqlServer("sql-server")
    .WithDataVolume()
    .AddDatabase("database");

var cache = builder.AddRedis("cache")
    .WithRedisInsight();

var api = builder.AddDotnetProject("api", "../FakeSurveyGenerator.Api/FakeSurveyGenerator.Api.csproj")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache)
    .WithHttpHealthCheck("health/live")
    .WithHttpHealthCheck("health/ready")
    .WithExternalHttpEndpoints()
    .WithDaprSidecar(options =>
    {
        options.WithOptions(new DaprSidecarOptions
        {
            ResourcesPaths = ["../../../dapr/components"],
            PlacementHostAddress = "",
            SchedulerHostAddress = ""
        });
    });

builder.AddDotnetProject("worker", "../FakeSurveyGenerator.Worker/FakeSurveyGenerator.Worker.csproj")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache);

#pragma warning disable ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.AddViteApp("ui", "../../client/ui")
    .WithHttpsDeveloperCertificate()
    .WithDeveloperCertificateTrust(true)
    .WithReference(api)
    .WaitFor(api)
    .WithHttpsEndpoint(port: 3000, isProxied: false)
    .WithHttpHealthCheck();
#pragma warning restore ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

builder.Build().Run();
