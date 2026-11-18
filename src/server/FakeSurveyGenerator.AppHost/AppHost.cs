using CommunityToolkit.Aspire.Hosting.Dapr;
using FakeSurveyGenerator.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// Opt-in remote access over a Tailscale Service. See docs/running-on-a-tailnet.md.
var tailnetHost = builder.Configuration["Tailnet:Host"]?.Trim() is { Length: > 0 } host ? host : null;

var database = builder.AddSqlServer("sql-server")
    .WithDataVolume()
    .AddDatabase("database");

var cache = builder.AddRedis("cache")
    .WithRedisInsight(redisInsight =>
    {
        if (tailnetHost is not null)
        {
            // The Tailscale Service forwards fixed ports, so Redis Insight cannot use a random one.
            redisInsight.WithHostPort(Tailnet.RedisInsightPort);
        }
    });

if (tailnetHost is not null)
{
    // Redis adds its "Manage (Insights)" link outside URL callbacks, so add the tailnet link directly.
    cache.WithUrls(context => context.Urls.Add(new ResourceUrlAnnotation
    {
        Url = $"https://{tailnetHost}:{Tailnet.RedisInsightPort}/",
        DisplayText = "Manage (Insights) (tailnet)",
        DisplayOrder = 1000
    }));
}

var api = builder.AddDotnetProject("api", "../FakeSurveyGenerator.Api/FakeSurveyGenerator.Api.csproj")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache)
    .WithHttpHealthCheck("health/live")
    .WithHttpHealthCheck("health/ready")
    .WithExternalHttpEndpoints()
    .WithTailnetUrls(tailnetHost)
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
    .WithHttpHealthCheck()
    .WithTailnetUrls(tailnetHost);
#pragma warning restore ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

builder.Build().Run();
