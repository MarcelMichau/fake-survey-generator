using Azure.Core;
using Microsoft.Extensions.Configuration;

namespace FakeSurveyGenerator.Application.Infrastructure.TypeSafe;

/// <summary>
/// Supplies the bearer token sent to a SystemOne-compatible provider.
/// </summary>
internal interface ITypeSafeAuthorizationProvider
{
    /// <returns>The bearer token, or <see langword="null"/> when no credential is configured.</returns>
    ValueTask<string?> GetBearerTokenAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Authenticates against the hosted TypeSafe API with a static API key.
/// </summary>
internal sealed class ApiKeyTypeSafeAuthorizationProvider(IConfiguration configuration) : ITypeSafeAuthorizationProvider
{
    public ValueTask<string?> GetBearerTokenAsync(CancellationToken cancellationToken)
    {
        var apiKey = configuration["TYPESAFE_API_KEY"];
        return ValueTask.FromResult(string.IsNullOrWhiteSpace(apiKey) ? null : apiKey);
    }
}

/// <summary>
/// Authenticates against a Microsoft Foundry deployment (for example, Microsoft-Decision-1) with a Microsoft Entra token.
/// </summary>
internal sealed class AzureCredentialTypeSafeAuthorizationProvider(TokenCredential credential, TimeProvider timeProvider)
    : ITypeSafeAuthorizationProvider, IDisposable
{
    internal const string Scope = "https://cognitiveservices.azure.com/.default";

    // Developer credentials such as the Azure CLI do not cache tokens, so avoid acquiring one per request.
    private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private volatile CachedToken? _cachedToken;

    public async ValueTask<string?> GetBearerTokenAsync(CancellationToken cancellationToken)
    {
        if (TryGetCachedToken() is { } token) return token;

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (TryGetCachedToken() is { } refreshedToken) return refreshedToken;

            var accessToken = await credential.GetTokenAsync(new TokenRequestContext([Scope]), cancellationToken);
            _cachedToken = new CachedToken(accessToken.Token, accessToken.ExpiresOn - RefreshMargin);
            return accessToken.Token;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public void Dispose() => _refreshLock.Dispose();

    private string? TryGetCachedToken()
    {
        var cachedToken = _cachedToken;
        return cachedToken is not null && timeProvider.GetUtcNow() < cachedToken.RefreshAt ? cachedToken.Value : null;
    }

    private sealed record CachedToken(string Value, DateTimeOffset RefreshAt);
}
