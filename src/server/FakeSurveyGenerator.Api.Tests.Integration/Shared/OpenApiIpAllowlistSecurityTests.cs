using System.Net;
using FakeSurveyGenerator.Api.Tests.Integration.Setup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

/// <summary>
/// Tests specifically for IP allowlist security functionality.
/// These tests simulate scenarios where access should be denied.
/// </summary>
public sealed class OpenApiIpAllowlistSecurityTests
{
    [Test]
    public async Task GivenStrictIpAllowlistWithCustomClientIp_WhenRequestingApiDocs_ThenAccessShouldBeDenied()
    {
        // Arrange
        var allowedIps = new[] { "192.168.1.100" };
        var forbiddenIp = "203.0.113.50"; // Different IP not in allowlist
        var settings = CreateDefaultTestContainerSettings();
        
        using var factory = new IntegrationTestWebApplicationFactoryWithStrictIpAllowlist(settings, allowedIps);
        using var client = factory.CreateClient();
        
        // Simulate a request from a different IP by adding headers
        client.DefaultRequestHeaders.Add("X-Forwarded-For", forbiddenIp);

        // Act
        var response = await client.GetAsync("/api-docs");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        
        var content = await response.Content.ReadAsStringAsync();
        await Assert.That(content).Contains("Access denied: IP address not in allowlist");
    }

    [Test]
    public async Task GivenStrictIpAllowlistWithAllowedClientIp_WhenRequestingApiDocs_ThenAccessShouldBeGranted()
    {
        // Arrange
        var allowedIps = new[] { "192.168.1.100", "203.0.113.42" };
        var allowedIp = "192.168.1.100";
        var settings = CreateDefaultTestContainerSettings();
        
        using var factory = new IntegrationTestWebApplicationFactoryWithStrictIpAllowlist(settings, allowedIps);
        using var client = factory.CreateClient();
        
        // Simulate a request from an allowed IP
        client.DefaultRequestHeaders.Add("X-Forwarded-For", allowedIp);

        // Act
        var response = await client.GetAsync("/api-docs");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenStrictIpAllowlistWithCustomClientIp_WhenRequestingOpenApiJson_ThenAccessShouldBeDenied()
    {
        // Arrange
        var allowedIps = new[] { "10.0.0.5" };
        var forbiddenIp = "172.16.0.10"; // Different IP not in allowlist
        var settings = CreateDefaultTestContainerSettings();
        
        using var factory = new IntegrationTestWebApplicationFactoryWithStrictIpAllowlist(settings, allowedIps);
        using var client = factory.CreateClient();
        
        // Simulate a request from a forbidden IP
        client.DefaultRequestHeaders.Add("X-Forwarded-For", forbiddenIp);

        // Act
        var response = await client.GetAsync("/openapi/v1.json");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GivenStrictIpAllowlistWithXRealIpHeader_WhenRequestingApiDocs_ThenHeaderShouldBeProcessed()
    {
        // Arrange
        var allowedIps = new[] { "198.51.100.5" };
        var allowedIp = "198.51.100.5";
        var settings = CreateDefaultTestContainerSettings();
        
        using var factory = new IntegrationTestWebApplicationFactoryWithStrictIpAllowlist(settings, allowedIps);
        using var client = factory.CreateClient();
        
        // Simulate a request using X-Real-IP header instead of X-Forwarded-For
        client.DefaultRequestHeaders.Add("X-Real-IP", allowedIp);

        // Act
        var response = await client.GetAsync("/api-docs");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenStrictIpAllowlistWithMultipleForwardedIps_WhenRequestingApiDocs_ThenFirstIpShouldBeUsed()
    {
        // Arrange
        var allowedIps = new[] { "203.0.113.1" };
        var firstIp = "203.0.113.1"; // This should be allowed
        var secondIp = "203.0.113.2"; // This would be forbidden but shouldn't matter
        var settings = CreateDefaultTestContainerSettings();
        
        using var factory = new IntegrationTestWebApplicationFactoryWithStrictIpAllowlist(settings, allowedIps);
        using var client = factory.CreateClient();
        
        // Simulate multiple IPs in X-Forwarded-For (comma-separated)
        client.DefaultRequestHeaders.Add("X-Forwarded-For", $"{firstIp}, {secondIp}");

        // Act
        var response = await client.GetAsync("/api-docs");

        // Assert - Should succeed because first IP is allowed
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenStrictIpAllowlistWithIpv6Address_WhenRequestingApiDocs_ThenIpv6ShouldBeProcessedCorrectly()
    {
        // Arrange
        var allowedIps = new[] { "2001:db8::1" };
        var allowedIpv6 = "2001:db8::1";
        var settings = CreateDefaultTestContainerSettings();
        
        using var factory = new IntegrationTestWebApplicationFactoryWithStrictIpAllowlist(settings, allowedIps);
        using var client = factory.CreateClient();
        
        // Simulate IPv6 request
        client.DefaultRequestHeaders.Add("X-Forwarded-For", allowedIpv6);

        // Act
        var response = await client.GetAsync("/api-docs");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GivenStrictIpAllowlistWithForbiddenIpv6_WhenRequestingApiDocs_ThenAccessShouldBeDenied()
    {
        // Arrange
        var allowedIps = new[] { "2001:db8::1" };
        var forbiddenIpv6 = "2001:db8::2"; // Different IPv6 address
        var settings = CreateDefaultTestContainerSettings();
        
        using var factory = new IntegrationTestWebApplicationFactoryWithStrictIpAllowlist(settings, allowedIps);
        using var client = factory.CreateClient();
        
        client.DefaultRequestHeaders.Add("X-Forwarded-For", forbiddenIpv6);

        // Act
        var response = await client.GetAsync("/api-docs");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GivenStrictIpAllowlist_WhenRequestingNonOpenApiRoute_ThenMiddlewareShouldNotInterfere()
    {
        // Arrange
        var allowedIps = new[] { "192.168.1.100" };
        var forbiddenIp = "203.0.113.50";
        var settings = CreateDefaultTestContainerSettings();
        
        using var factory = new IntegrationTestWebApplicationFactoryWithStrictIpAllowlist(settings, allowedIps);
        using var client = factory.CreateClient();
        
        // Simulate request from forbidden IP to a non-OpenAPI route
        client.DefaultRequestHeaders.Add("X-Forwarded-For", forbiddenIp);

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert - Should succeed because IP filtering only applies to OpenAPI routes
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
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

/// <summary>
/// Inherits from IntegrationTestWebApplicationFactory to ensure proper TestContainerSettings configuration.
/// This version creates a strict IP allowlist environment for testing security scenarios.
/// </summary>
internal sealed class IntegrationTestWebApplicationFactoryWithStrictIpAllowlist(TestContainerSettings settings, string[] allowedIps) 
    : IntegrationTestWebApplicationFactory(settings)
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Call base configuration first to ensure proper setup
        base.ConfigureWebHost(builder);
        
        // Add strict IP allowlist configuration (must have at least one IP to activate middleware)
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var ipConfiguration = new Dictionary<string, string>();
            
            // Ensure we always have at least one IP configured to activate the middleware
            var ipsToAdd = allowedIps.Length > 0 ? allowedIps : new[] { "192.168.1.100" };
            
            for (int i = 0; i < ipsToAdd.Length; i++)
            {
                ipConfiguration.Add($"OpenApi:AllowedIPs:{i}", ipsToAdd[i]);
            }

            config.AddInMemoryCollection(ipConfiguration!);
        });
    }
}