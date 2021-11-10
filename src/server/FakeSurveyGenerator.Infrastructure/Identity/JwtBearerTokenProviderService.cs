using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace FakeSurveyGenerator.Infrastructure.Identity;

internal sealed class JwtBearerTokenProviderService : ITokenProviderService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtBearerTokenProviderService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public string GetToken()
    {
        if (_httpContextAccessor.HttpContext is null)
            throw new InvalidOperationException("Tried to get a JWT from outside an HttpContext");

        if (_httpContextAccessor.HttpContext.User.Identity is { IsAuthenticated: false })
            throw new InvalidOperationException("Cannot retrieve a token for an unauthorized user");

        var authenticationHeaderValue = AuthenticationHeaderValue.Parse(_httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization]);

        return authenticationHeaderValue.Parameter;
    }
}