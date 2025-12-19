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

builder.AddViteApp("ui", "../../client/ui")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpsEndpoint(port: 3000, isProxied: false);

builder.Build().Run();