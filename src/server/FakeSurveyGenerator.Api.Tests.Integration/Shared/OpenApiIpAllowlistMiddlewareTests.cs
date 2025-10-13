using System.Net;
using FakeSurveyGenerator.Api.Configuration.OpenApi;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FakeSurveyGenerator.Api.Tests.Integration.Shared;

/// <summary>
/// Unit tests for the OpenApiIpAllowlistMiddleware to test its logic in isolation.
/// </summary>
public sealed class OpenApiIpAllowlistMiddlewareTests
{
    private readonly ILogger<OpenApiIpAllowlistMiddleware> _mockLogger = Substitute.For<ILogger<OpenApiIpAllowlistMiddleware>>();

    [Test]
    public async Task GivenAllowedIpInList_WhenInvokingMiddleware_ThenNextShouldBeCalled()
    {
        // Arrange
        var allowedIps = new[] { "192.168.1.100", "203.0.113.42" };
        var nextWasCalled = false;
        
        RequestDelegate next = (context) =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new OpenApiIpAllowlistMiddleware(next, allowedIps, _mockLogger);
        var context = CreateHttpContext("192.168.1.100");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        await Assert.That(nextWasCalled).IsTrue();
        await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status200OK);
    }

    [Test]
    public async Task GivenLocalhostIp_WhenInvokingMiddleware_ThenNextShouldBeCalled()
    {
        // Arrange
        var allowedIps = new[] { "192.168.1.100" }; // Localhost not explicitly in list
        var nextWasCalled = false;

        var middleware = new OpenApiIpAllowlistMiddleware(Next, allowedIps, _mockLogger);
        var context = CreateHttpContext("127.0.0.1");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        await Assert.That(nextWasCalled).IsTrue();
        await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status200OK);
        return;

        Task Next(HttpContext httpContext)
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        }
    }

    [Test]
    public async Task GivenIpv6Localhost_WhenInvokingMiddleware_ThenNextShouldBeCalled()
    {
        // Arrange
        var allowedIps = new[] { "192.168.1.100" };
        var nextWasCalled = false;
        
        RequestDelegate next = (context) =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new OpenApiIpAllowlistMiddleware(next, allowedIps, _mockLogger);
        var context = CreateHttpContext("::1");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        await Assert.That(nextWasCalled).IsTrue();
        await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status200OK);
    }

    [Test]
    public async Task GivenEmptyAllowedIpsArray_WhenInvokingWithLocalhost_ThenNextShouldBeCalled()
    {
        // Arrange
        var allowedIps = Array.Empty<string>();
        var nextWasCalled = false;
        
        RequestDelegate next = (context) =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new OpenApiIpAllowlistMiddleware(next, allowedIps, _mockLogger);
        var context = CreateHttpContext("127.0.0.1");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        await Assert.That(nextWasCalled).IsTrue();
    }

    [Test]
    public async Task GivenXForwardedForHeader_WhenInvokingMiddleware_ThenForwardedIpShouldBeUsed()
    {
        // Arrange
        var allowedIps = new[] { "203.0.113.42" };
        var nextWasCalled = false;
        
        RequestDelegate next = (context) =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new OpenApiIpAllowlistMiddleware(next, allowedIps, _mockLogger);
        var context = CreateHttpContextWithForwardedHeader("203.0.113.42");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        await Assert.That(nextWasCalled).IsTrue();
    }

    [Test]
    public async Task GivenMultipleIpsInXForwardedFor_WhenInvokingMiddleware_ThenFirstIpShouldBeUsed()
    {
        // Arrange
        var allowedIps = new[] { "203.0.113.42" };
        var nextWasCalled = false;
        
        RequestDelegate next = (context) =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new OpenApiIpAllowlistMiddleware(next, allowedIps, _mockLogger);
        var context = CreateHttpContext("127.0.0.1");
        context.Request.Headers["X-Forwarded-For"] = "203.0.113.42, 198.51.100.1, 172.16.0.1";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        await Assert.That(nextWasCalled).IsTrue();
    }

    [Test]
    public async Task GivenXRealIpHeader_WhenInvokingMiddleware_ThenRealIpShouldBeUsed()
    {
        // Arrange
        var allowedIps = new[] { "198.51.100.1" };
        var nextWasCalled = false;
        
        RequestDelegate next = (context) =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new OpenApiIpAllowlistMiddleware(next, allowedIps, _mockLogger);
        var context = CreateHttpContext("127.0.0.1");
        context.Request.Headers["X-Real-IP"] = "198.51.100.1";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        await Assert.That(nextWasCalled).IsTrue();
    }

    [Test]
    public async Task GivenWhitespaceInAllowedIps_WhenCreatingMiddleware_ThenWhitespaceShouldBeTrimmed()
    {
        // Arrange
        var allowedIps = new[] { "  192.168.1.100  ", "203.0.113.42" };
        var nextWasCalled = false;
        
        RequestDelegate next = (context) =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new OpenApiIpAllowlistMiddleware(next, allowedIps, _mockLogger);
        var context = CreateHttpContext("192.168.1.100");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        await Assert.That(nextWasCalled).IsTrue();
    }

    [Test]
    public async Task GivenInvalidIpInAllowedList_WhenCreatingMiddleware_ThenInvalidIpShouldBeIgnored()
    {
        // Arrange
        var allowedIps = new[] { "192.168.1.100", "invalid-ip-address", "203.0.113.42" };
        var nextWasCalled = false;
        
        RequestDelegate next = (context) =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new OpenApiIpAllowlistMiddleware(next, allowedIps, _mockLogger);
        var context = CreateHttpContext("203.0.113.42");

        // Act
        await middleware.InvokeAsync(context);

        // Assert - Should still work with valid IPs
        await Assert.That(nextWasCalled).IsTrue();
    }

    [Test]
    public async Task GivenNullRemoteIpAddress_WhenInvokingMiddleware_ThenAccessShouldBeDenied()
    {
        // Arrange
        var allowedIps = new[] { "192.168.1.100" };
        var nextWasCalled = false;
        
        RequestDelegate next = (context) =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new OpenApiIpAllowlistMiddleware(next, allowedIps, _mockLogger);
        var context = CreateHttpContextWithNullRemoteIp();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        await Assert.That(nextWasCalled).IsFalse();
        await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status403Forbidden);
    }

    private static HttpContext CreateHttpContext(string remoteIpAddress)
    {
        var context = new DefaultHttpContext
        {
            Connection =
            {
                RemoteIpAddress = IPAddress.Parse(remoteIpAddress)
            },
            Response =
            {
                Body = new MemoryStream()
            }
        };
        return context;
    }

    private static HttpContext CreateHttpContextWithForwardedHeader(string forwardedIp)
    {
        var context = new DefaultHttpContext
        {
            Connection =
            {
                RemoteIpAddress = IPAddress.Parse("127.0.0.1")
            }
        };
        context.Request.Headers["X-Forwarded-For"] = forwardedIp;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static HttpContext CreateHttpContextWithNullRemoteIp()
    {
        var context = new DefaultHttpContext
        {
            Connection =
            {
                RemoteIpAddress = null
            },
            Response =
            {
                Body = new MemoryStream()
            }
        };
        return context;
    }
}