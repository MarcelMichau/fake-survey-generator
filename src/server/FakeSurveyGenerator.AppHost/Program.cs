using System.Collections.Immutable;
using Aspire.Hosting.Dapr;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddSqlServer("sql-server")
    .AddDatabase("database");

var cache = builder.AddRedis("cache");

var api = builder.AddProject<FakeSurveyGenerator_Api>("fakesurveygeneratorapi", "https")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache)
    .WithDaprSidecar(options =>
    {
        options.WithOptions(new DaprSidecarOptions
        {
            ResourcesPaths = ImmutableHashSet.Create("../../../dapr/components")
        });
    });

builder.AddNpmApp("fake-survey-generator-ui", "../../client/ui", "dev")
    .WithReference(api)
    .WaitFor(api)
    .WithEndpoint(targetPort: 3000, port: 3000, scheme: "https", env: "PORT", isProxied: false)
    .PublishAsDockerFile();

builder.Build().Run();