extern alias AppHost;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using AppHostProject = AppHost::Projects.FakeSurveyGenerator_AppHost;

namespace FakeSurveyGenerator.Api.Tests.Integration.Setup;

/// <summary>
/// A custom <see cref="DistributedApplicationFactory"/> that starts only the infrastructure
/// resources (SQL Server and Redis) needed for integration tests. Project resources (api, worker,
/// ui) and Redis Insight are excluded from automatic startup via
/// <see cref="ExplicitStartupAnnotation"/>. The SQL Server data volume is also removed so each
/// test run starts with a clean database.
/// </summary>
public sealed class TestingAspireAppHost()
    : DistributedApplicationFactory(typeof(AppHostProject))
{
    protected override void OnBuilding(DistributedApplicationBuilder appBuilder)
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

        // Prevent project resources and Redis Insight from starting automatically —
        // the API is hosted via WebApplicationFactory and the others are not needed for tests.
        var resourcesToSkip = appBuilder.Resources
            .Where(r => r is ProjectResource || r.Name is "redis-insight" or "ui")
            .ToList();

        foreach (var resource in resourcesToSkip)
            resource.Annotations.Add(new ExplicitStartupAnnotation());
    }
}
