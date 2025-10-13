# OpenAPI IP Allowlist Integration Tests

This document describes the comprehensive test suite for the OpenAPI IP allowlist middleware functionality.

## Overview

The IP allowlist middleware restricts access to OpenAPI documentation endpoints (`/api-docs` and `/openapi/*`) based on client IP addresses. The tests ensure that:

1. **Security**: Only allowed IP addresses can access OpenAPI documentation
2. **Flexibility**: Both IPv4 and IPv6 addresses are supported
3. **Development-Friendly**: Localhost addresses are always allowed for development
4. **Proxy-Aware**: Properly handles `X-Forwarded-For` and `X-Real-IP` headers
5. **Non-Intrusive**: Other API endpoints are not affected by IP restrictions

## Test Structure

### 1. OpenApiTests.cs
Contains the original basic functionality tests:
- Basic access to `/api-docs` and `/openapi/v1.json` routes
- Ensures existing functionality continues to work

### 2. OpenApiIpAllowlistTests.cs
Contains integration tests for the IP allowlist functionality:
- **Configuration Testing**: Various IP allowlist configurations
- **IPv4/IPv6 Support**: Mixed IP address type testing  
- **Input Validation**: Handling of invalid IPs and whitespace
- **Route Isolation**: Ensures middleware only affects OpenAPI routes

### 3. OpenApiIpAllowlistSecurityTests.cs
Contains security-focused tests that simulate real-world scenarios:
- **Access Denial**: Tests where access should be explicitly denied
- **Header Processing**: Tests `X-Forwarded-For` and `X-Real-IP` header handling
- **Multiple IPs**: Tests handling of comma-separated forwarded IPs
- **IPv6 Security**: IPv6-specific security scenarios

### 4. OpenApiIpAllowlistMiddlewareTests.cs
Contains unit tests for the middleware logic in isolation:
- **IP Parsing**: Tests IP address parsing and validation
- **Header Priority**: Tests precedence of different IP headers
- **Edge Cases**: Null IPs, invalid formats, etc.

## Configuration

The IP allowlist is configured via `appsettings.json`:

```json
{
  "OpenApi": {
    "AllowedIPs": [
      "192.168.1.100",
      "10.0.0.5", 
      "203.0.113.42",
      "::1",
      "2001:db8::1"
    ]
  }
}
```

## Key Features Tested

### 1. Multiple IP Address Support
- IPv4 addresses (e.g., `192.168.1.100`)
- IPv6 addresses (e.g., `2001:db8::1`)
- Mixed IPv4/IPv6 configurations

### 2. Proxy Header Support
- `X-Forwarded-For`: `clientIP, proxy1, proxy2` (uses first IP)
- `X-Real-IP`: `clientIP` (alternative header)
- Fallback to `HttpContext.Connection.RemoteIpAddress`

### 3. Development-Friendly Defaults
- `127.0.0.1` (IPv4 localhost) always allowed
- `::1` (IPv6 localhost) always allowed
- Supports local development without configuration

### 4. Input Validation
- Invalid IP addresses are ignored (not rejected)
- Whitespace is trimmed from IP addresses
- Empty/null IP configurations handled gracefully

### 5. Route Isolation
- Only `/api-docs` and `/openapi/*` routes are protected
- Other API endpoints (`/api/*`, `/health/*`) remain unaffected

## Test Factories

### TestWebApplicationFactoryWithIpAllowlist
- Configures specific IP allowlists for testing
- Used for positive test cases (access granted)

### TestWebApplicationFactoryWithoutIpConfig  
- No IP configuration provided
- Tests default behavior (localhost access)

### StrictTestWebApplicationFactory
- More production-like environment
- Used for security tests with custom client IP simulation
- Tests access denial scenarios

## Running the Tests

```bash
# Run all OpenAPI tests
dotnet test --filter "FullyQualifiedName~OpenApi"

# Run only IP allowlist tests  
dotnet test --filter "FullyQualifiedName~IpAllowlist"

# Run specific test class
dotnet test --filter "FullyQualifiedName~OpenApiIpAllowlistSecurityTests"
```

## Security Considerations

1. **Default Security**: Without configuration, only localhost is allowed
2. **Proxy Awareness**: Properly handles load balancer/reverse proxy scenarios
3. **Header Validation**: Validates and parses IP addresses from headers
4. **Logging**: Security events are logged for monitoring
5. **Graceful Failures**: Invalid configurations don't break the application

## Future Enhancements

The test suite is designed to easily accommodate future features:
- CIDR range support (`192.168.1.0/24`)
- Time-based access restrictions
- Rate limiting integration
- Geographic IP restrictions
- Dynamic IP allowlist updates