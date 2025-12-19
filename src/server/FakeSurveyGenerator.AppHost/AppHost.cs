using CommunityToolkit.Aspire.Hosting.Dapr;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddSqlServer("sql-server")
    .WithDataVolume()
    .AddDatabase("database");

var cache = builder.AddRedis("cache")
    .WithRedisInsight();

var api = builder.AddProject<FakeSurveyGenerator_Api>("api")
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
            ResourcesPaths = ["../../../dapr/components"]
        });
    });

var worker = builder.AddProject<FakeSurveyGenerator_Worker>("worker")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache);

#pragma warning disable ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.AddViteApp("ui", "../../client/frontend")
    .WithHttpsDeveloperCertificate()
    .WithDeveloperCertificateTrust(true)
    .WithReference(api)
    .WaitFor(api)
    .WithHttpsEndpoint(port: 3000, isProxied: false);
#pragma warning restore ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

builder.Build().Run();