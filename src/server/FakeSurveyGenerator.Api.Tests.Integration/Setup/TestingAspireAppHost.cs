extern alias AppHost;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.JavaScript;
using Aspire.Hosting.Redis;
using Aspire.Hosting.Testing;
using AppHostProject = AppHost::Projects.FakeSurveyGenerator_AppHost;

namespace FakeSurveyGenerator.Api.Tests.Integration.Setup;

/// <summary>
/// A custom <see cref="DistributedApplicationFactory"/> that starts only the infrastructure
/// resources (SQL Server and Redis) needed for integration tests. Project resources (api, worker,
/// ui), JavaScript package installers, and Redis Insight are excluded from automatic startup via
/// <see cref="ExplicitStartupAnnotation"/>. The SQL Server data volume is also removed so each
/// test run starts with a clean database.
/// </summary>
public sealed class TestingAspireAppHost()
    : DistributedApplicationFactory(typeof(AppHostProject))
{
    public DistributedApplication? App { get; private set; }

    protected override void OnBuilt(DistributedApplication application)
    {
        App = application;
    }

    protected override void OnBuilding(DistributedApplicationBuilder appBuilder)
    {
        ConfigureResourcesForIntegrationTests(appBuilder);
    }

    internal static void ConfigureResourcesForIntegrationTests(IDistributedApplicationBuilder appBuilder)
    {
        // Remove the persistent data volume from SQL Server so each test run starts fresh
        var sqlServer = appBuilder.Resources.FirstOrDefault(r => r.Name == "sql-server");
        if (sqlServer is not null)
        {
            var volumeAnnotation = sqlServer.Annotations
                .OfType<ContainerMountAnnotation>()
                .FirstOrDefault(a => a.Type == ContainerMountType.Volume);

            if (volumeAnnotation is not null)
                sqlServer.Annotations.Remove(volumeAnnotation);
        }

        // The API is hosted via WebApplicationFactory; other app resources are not needed.
        // Defer the separate JavaScript installers too: otherwise integration and acceptance
        // test hosts can run npm install concurrently against the same node_modules directory.
        var resourcesToSkip = appBuilder.Resources
            .Where(r => r is ProjectResource or RedisInsightResource or JavaScriptInstallerResource || r.Name == "ui")
            .ToList();

        foreach (var resource in resourcesToSkip)
            resource.Annotations.Add(new ExplicitStartupAnnotation());
    }
}
