using System.Net;
using FakeSurveyGenerator.Api.Tests.Integration.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

/// <summary>
/// Inherits from IntegrationTestWebApplicationFactory to ensure proper TestContainerSettings configuration
/// while adding IP allowlist configuration for testing.
/// </summary>
internal sealed class IntegrationTestWebApplicationFactoryWithIpAllowlist(TestContainerSettings settings, string[] allowedIps) 
    : IntegrationTestWebApplicationFactory(settings)
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Call base configuration first to ensure proper setup
        base.ConfigureWebHost(builder);
        
        // Add IP allowlist configuration
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var ipConfiguration = new Dictionary<string, string>();
            
            // Add IP allowlist configuration
            for (int i = 0; i < allowedIps.Length; i++)
            {
                ipConfiguration.Add($"OpenApi:AllowedIPs:{i}", allowedIps[i]);
            }

            config.AddInMemoryCollection(ipConfiguration!);
        });
    }
}

public sealed class OpenApiTests
{
    [ClassDataSource<IntegrationTestFixture>(Shared = SharedType.PerTestSession)]
    public required IntegrationTestFixture TestFixture { get; init; }

    private HttpClient Client => TestFixture.Factory!.CreateClient();

    [Test]
    public async Task GivenAnyUser_WhenMakingRequestToApiDocsRoute_ThenSuccessResponseShouldBeReturned()
    {
        var response = await Client.GetAsync("/api-docs");
        response.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task GivenAnyUser_WhenMakingRequestToOpenApiJsonRoute_ThenSuccessResponseShouldBeReturned()
    {
        var response = await Client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();
    }
}

public sealed class OpenApiIpAllowlistTests
{
    private static IntegrationTestWebApplicationFactoryWithIpAllowlist CreateFactoryWithIpAllowlist(string[] allowedIps, TestContainerSettings settings)
    {
        return new IntegrationTestWebApplicationFactoryWithIpAllowlist(settings, allowedIps);
    }

    [Test]
    public async Task GivenAllowedIpInConfiguration_WhenRequestingApiDocs_ThenAccessShouldBeGranted()
    {
        // Arrange
        var allowedIps = new[] { "192.168.1.100", "10.0.0.5" };
        var settings = CreateDefaultTestContainerSettings();
        using var factory = CreateFactoryWithIpAllowlist(allowedIps, settings);
        using var client = factory.CreateClient();
        
        // Simulate request from an allowed IP
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "192.168.1.100");

        // Act
        var response = await client.GetAsync("/api-docs");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenEmptyIpAllowlist_WhenRequestingApiDocs_ThenAccessShouldBeGrantedForLocalhost()
    {
        // Arrange - Use base factory with no IP configuration at all
        var settings = CreateDefaultTestContainerSettings();
        using var factory = new IntegrationTestWebApplicationFactory(settings);
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api-docs");

        // Assert - Should pass because no IP restrictions are applied when no configuration exists
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenAllowedIpInConfiguration_WhenRequestingOpenApiJson_ThenAccessShouldBeGranted()
    {
        // Arrange
        var allowedIps = new[] { "192.168.1.100", "203.0.113.42" };
        var settings = CreateDefaultTestContainerSettings();
        using var factory = CreateFactoryWithIpAllowlist(allowedIps, settings);
        using var client = factory.CreateClient();
        
        // Simulate request from an allowed IP
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "203.0.113.42");

        // Act
        var response = await client.GetAsync("/openapi/v1.json");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenIpAllowlistEnabled_WhenRequestingNonOpenApiRoute_ThenAccessShouldNotBeRestricted()
    {
        // Arrange
        var allowedIps = new[] { "192.168.1.100" };
        var settings = CreateDefaultTestContainerSettings();
        using var factory = CreateFactoryWithIpAllowlist(allowedIps, settings);
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert - Health endpoints should not be restricted by OpenAPI middleware
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenAllowedIpInConfiguration_WhenRequestingApiDocsWithTrailingSlash_ThenAccessShouldBeGranted()
    {
        // Arrange
        var allowedIps = new[] { "192.168.1.100" };
        var settings = CreateDefaultTestContainerSettings();
        using var factory = CreateFactoryWithIpAllowlist(allowedIps, settings);
        using var client = factory.CreateClient();
        
        // Simulate request from an allowed IP
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "192.168.1.100");

        // Act
        var response = await client.GetAsync("/api-docs/");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenIpv6AddressInAllowlist_WhenRequestingOpenApiRoutes_ThenAccessShouldBeGranted()
    {
        // Arrange
        var allowedIps = new[] { "::1", "2001:db8::1" };
        var settings = CreateDefaultTestContainerSettings();
        using var factory = CreateFactoryWithIpAllowlist(allowedIps, settings);
        using var client = factory.CreateClient();
        
        // Simulate request from an allowed IPv6 address
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "2001:db8::1");

        // Act & Assert
        var apiDocsResponse = await client.GetAsync("/api-docs");
        await Assert.That(apiDocsResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        // Clear headers and set for second request
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "::1");
        
        var openApiResponse = await client.GetAsync("/openapi/v1.json");
        await Assert.That(openApiResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenMixedIpv4AndIpv6InAllowlist_WhenRequestingOpenApiRoutes_ThenAccessShouldBeGranted()
    {
        // Arrange
        var allowedIps = new[] { "192.168.1.100", "::1", "203.0.113.42", "2001:db8::1" };
        var settings = CreateDefaultTestContainerSettings();
        using var factory = CreateFactoryWithIpAllowlist(allowedIps, settings);
        using var client = factory.CreateClient();
        
        // Simulate request from an allowed IPv4 address
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "192.168.1.100");

        // Act & Assert
        var response = await client.GetAsync("/api-docs");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenWhitespaceInIpConfiguration_WhenRequestingOpenApiRoutes_ThenInvalidIpsShouldBeIgnored()
    {
        // Arrange
        var allowedIps = new[] { "  192.168.1.100  ", "", "   ", "203.0.113.42" };
        var settings = CreateDefaultTestContainerSettings();
        using var factory = CreateFactoryWithIpAllowlist(allowedIps, settings);
        using var client = factory.CreateClient();
        
        // Simulate request from an allowed IP (after whitespace trimming)
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "192.168.1.100");

        // Act
        var response = await client.GetAsync("/api-docs");

        // Assert - Should work because valid IPs are processed correctly
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenInvalidIpAddressInConfiguration_WhenRequestingOpenApiRoutes_ThenInvalidIpsShouldBeIgnored()
    {
        // Arrange
        var allowedIps = new[] { "192.168.1.100", "invalid-ip", "203.0.113.42", "999.999.999.999" };
        var settings = CreateDefaultTestContainerSettings();
        using var factory = CreateFactoryWithIpAllowlist(allowedIps, settings);
        using var client = factory.CreateClient();
        
        // Simulate request from a valid allowed IP
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "203.0.113.42");

        // Act
        var response = await client.GetAsync("/api-docs");

        // Assert - Should work because valid IPs are processed and localhost is always allowed
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenNoIpConfiguration_WhenRequestingOpenApiRoutes_ThenLocalhostAccessShouldBeAllowed()
    {
        // Arrange - No IP configuration at all (using base factory)
        var settings = CreateDefaultTestContainerSettings();
        using var factory = new IntegrationTestWebApplicationFactory(settings);
        using var client = factory.CreateClient();

        // Act & Assert
        var apiDocsResponse = await client.GetAsync("/api-docs");
        await Assert.That(apiDocsResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var openApiResponse = await client.GetAsync("/openapi/v1.json");
        await Assert.That(openApiResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    private static TestContainerSettings CreateDefaultTestContainerSettings()
    {
        // Use in-memory database for testing to avoid needing actual containers
        return new TestContainerSettings(
            SqlServerConnectionString: "Server=(localdb)\\mssqllocaldb;Database=FakeSurveyGeneratorTest;Trusted_Connection=true;MultipleActiveResultSets=true;",
            RedisConnectionString: "localhost:6379"
        );
    }
}