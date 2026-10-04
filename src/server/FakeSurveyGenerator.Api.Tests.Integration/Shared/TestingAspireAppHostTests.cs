extern alias AppHost;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.JavaScript;
using Aspire.Hosting.Testing;
using FakeSurveyGenerator.Api.Tests.Integration.Setup;
using AppHostProject = AppHost::Projects.FakeSurveyGenerator_AppHost;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

public sealed class TestingAspireAppHostTests
{
    [Test]
    public async Task GivenIntegrationTestHost_WhenConfiguringResources_ThenSqlServerDataVolumeIsRemoved()
    {
        await using var builder = await DistributedApplicationTestingBuilder.CreateAsync<AppHostProject>();
        var sqlServer = builder.Resources.Single(resource => resource.Name == "sql-server");

        // Local development keeps persistent data; integration tests must not reuse it.
        await Assert.That(sqlServer.Annotations.OfType<ContainerMountAnnotation>()
            .Any(annotation => annotation.Type == ContainerMountType.Volume)).IsTrue();

        TestingAspireAppHost.ConfigureResourcesForIntegrationTests(builder);

        await Assert.That(sqlServer.Annotations.OfType<ContainerMountAnnotation>()
            .Any(annotation => annotation.Type == ContainerMountType.Volume)).IsFalse();
    }

    [Test]
    public async Task GivenIntegrationTestHost_WhenConfiguringResources_ThenUiInstallerIsDeferredAndInfrastructureRemainsAutomatic()
    {
        // Inspect the real AppHost model without starting processes or containers.
        await using var builder = await DistributedApplicationTestingBuilder.CreateAsync<AppHostProject>();
        var installer = builder.Resources.OfType<JavaScriptInstallerResource>().Single();
        var ui = builder.Resources.Single(resource => resource.Name == "ui");

        // Guard against a change that disables installation in the production AppHost itself.
        await Assert.That(installer.Annotations.OfType<ExplicitStartupAnnotation>().Any()).IsFalse();
        await Assert.That(ui.Annotations.OfType<ExplicitStartupAnnotation>().Any()).IsFalse();

        TestingAspireAppHost.ConfigureResourcesForIntegrationTests(builder);

        await Assert.That(installer.Annotations.OfType<ExplicitStartupAnnotation>().Any()).IsTrue();
        await Assert.That(ui.Annotations.OfType<ExplicitStartupAnnotation>().Any()).IsTrue();

        foreach (var name in new[] { "sql-server", "database", "cache" })
        {
            var resource = builder.Resources.Single(resource => resource.Name == name);
            await Assert.That(resource.Annotations.OfType<ExplicitStartupAnnotation>().Any()).IsFalse();
        }
    }
}
