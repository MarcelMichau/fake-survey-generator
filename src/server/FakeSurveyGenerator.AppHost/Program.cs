using System.Collections.Immutable;
using Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddSqlServer("sql-server")
    .AddDatabase("database")
    .WithEndpoint(containerPort: 1433, hostPort: 1433);

var cache = builder.AddRedis("cache");

var api = builder.AddProject<Projects.FakeSurveyGenerator_Api>("fakesurveygeneratorapi")
    .WithLaunchProfile("https")
    .WithReference(database)
    .WithReference(cache)
    .WithDaprSidecar(options =>
    {
        options.WithOptions(new DaprSidecarOptions
        {
            ResourcesPaths = ImmutableHashSet.Create("../dapr/components")
        });
    });

builder.AddNpmApp("fake-survey-generator-ui", "../src/client/ui", "dev")
    .WithReference(api)
    .WithEndpoint(containerPort: 3000, hostPort: 3000, scheme: "https", env: "PORT")
    .PublishAsDockerFile();

builder.Build().Run();
