using Azure.Core;
using FakeSurveyGenerator.Application.Infrastructure.SurveyAnalysis;
using Microsoft.Extensions.Time.Testing;

namespace FakeSurveyGenerator.Application.Tests.Infrastructure.SurveyAnalysis;

public sealed class SurveyAnalysisTokenProviderTests
{
    [Test]
    public async Task GivenAFreshToken_WhenRequestingAgain_ThenReusesTheCachedToken()
    {
        var timeProvider = new FakeTimeProvider();
        var credential = new CountingCredential(timeProvider, TimeSpan.FromHours(1));
        using var provider = new AzureCredentialSurveyAnalysisTokenProvider(credential, timeProvider);

        var first = await provider.GetTokenAsync(CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromMinutes(54));
        var second = await provider.GetTokenAsync(CancellationToken.None);

        await Assert.That(first).IsEqualTo("token-1");
        await Assert.That(second).IsEqualTo("token-1");
        await Assert.That(credential.RequestCount).IsEqualTo(1);
        await Assert.That(credential.LastScopes).IsEquivalentTo(["https://cognitiveservices.azure.com/.default"]);
    }

    [Test]
    public async Task GivenATokenNearExpiry_WhenRequesting_ThenAcquiresANewToken()
    {
        var timeProvider = new FakeTimeProvider();
        var credential = new CountingCredential(timeProvider, TimeSpan.FromHours(1));
        using var provider = new AzureCredentialSurveyAnalysisTokenProvider(credential, timeProvider);

        await provider.GetTokenAsync(CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromMinutes(55));
        var refreshed = await provider.GetTokenAsync(CancellationToken.None);

        await Assert.That(refreshed).IsEqualTo("token-2");
        await Assert.That(credential.RequestCount).IsEqualTo(2);
    }

    private sealed class CountingCredential(TimeProvider timeProvider, TimeSpan lifetime) : TokenCredential
    {
        public int RequestCount { get; private set; }
        public string[] LastScopes { get; private set; } = [];

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            RequestCount++;
            LastScopes = requestContext.Scopes;
            return ValueTask.FromResult(new AccessToken($"token-{RequestCount}", timeProvider.GetUtcNow() + lifetime));
        }
    }
}
