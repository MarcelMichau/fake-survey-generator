using Aspire.Hosting.Yarp.Transforms;
using CommunityToolkit.Aspire.Hosting.Dapr;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddSqlServer("sql-server")
    .WithDataVolume()
    .AddDatabase("database");

var cache = builder.AddRedis("cache")
    .WithRedisInsight();

var api = builder.AddProject<FakeSurveyGenerator_Api>("fakesurveygeneratorapi", "https")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache)
    .WithHttpHealthCheck("health/live")
    .WithHttpHealthCheck("health/ready")
    .WithDaprSidecar(options =>
    {
        options.WithOptions(new DaprSidecarOptions
        {
            ResourcesPaths = ["../../../dapr/components"]
        });
    });

var worker = builder.AddProject<FakeSurveyGenerator_Worker>("fake-survey-generator-worker")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache);

var ui = builder.AddBunApp("fake-survey-generator-ui", "../../client/ui", "dev")
    .WithBunPackageInstallation()
    .WithReference(api)
    .WaitFor(api)
    .WithEndpoint(targetPort: 3000, scheme: "https", env: "PORT", name: "https")
    .PublishAsDockerFile();

var gateway = builder.AddProject<FakeSurveyGenerator_Gateway>("gateway")
    .WithReference(api)
    .WaitFor(api)
    .WithReference(ui.GetEndpoint("https"))
    .WaitFor(ui)
    .WithExternalHttpEndpoints();

builder.Build().Run();