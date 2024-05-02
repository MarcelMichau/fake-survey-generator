using System.Collections.Immutable;
using Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddSqlServer("sql-server")
    .AddDatabase("database")
    .WithEndpoint(targetPort: 1433, port: 1433);

var cache = builder.AddRedis("cache");

var api = builder.AddProject<Projects.FakeSurveyGenerator_Api>("fakesurveygeneratorapi", "https")
    .WithReference(database)
    .WithReference(cache)
    .WithDaprSidecar(options =>
    {
        options.WithOptions(new DaprSidecarOptions
        {
            ResourcesPaths = ImmutableHashSet.Create("../../../dapr/components")
        });
    });

builder.AddNpmApp("fake-survey-generator-ui", "../../client/ui", "dev")
    .WithReference(api)
    .WithEndpoint(targetPort: 3000, port: 3000, scheme: "https", env: "PORT", isProxied: false)
    .PublishAsDockerFile();

builder.Build().Run();