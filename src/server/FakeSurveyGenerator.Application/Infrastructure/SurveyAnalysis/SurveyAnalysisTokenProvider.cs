using Azure.Core;

namespace FakeSurveyGenerator.Application.Infrastructure.SurveyAnalysis;

/// <summary>
/// Supplies the Microsoft Entra bearer token sent to the Microsoft Foundry survey analysis deployment.
/// </summary>
internal interface ISurveyAnalysisTokenProvider
{
    ValueTask<string> GetTokenAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Acquires and caches Microsoft Entra tokens for Microsoft Foundry from an Azure credential.
/// </summary>
internal sealed class AzureCredentialSurveyAnalysisTokenProvider(TokenCredential credential, TimeProvider timeProvider)
    : ISurveyAnalysisTokenProvider, IDisposable
{
    internal const string Scope = "https://cognitiveservices.azure.com/.default";

    // Developer credentials such as the Azure CLI do not cache tokens, so avoid acquiring one per request.
    private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private volatile CachedToken? _cachedToken;

    public async ValueTask<string> GetTokenAsync(CancellationToken cancellationToken)
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
