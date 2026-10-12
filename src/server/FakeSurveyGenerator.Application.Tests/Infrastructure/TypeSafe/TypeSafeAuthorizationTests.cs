using Azure.Core;
using FakeSurveyGenerator.Application.Infrastructure.TypeSafe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace FakeSurveyGenerator.Application.Tests.Infrastructure.TypeSafe;

public sealed class TypeSafeAuthorizationTests
{
    [Test]
    public async Task GivenAFreshToken_WhenRequestingAgain_ThenReusesTheCachedToken()
    {
        var timeProvider = new FakeTimeProvider();
        var credential = new CountingCredential(timeProvider, TimeSpan.FromHours(1));
        using var provider = new AzureCredentialTypeSafeAuthorizationProvider(credential, timeProvider);

        var first = await provider.GetBearerTokenAsync(CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromMinutes(54));
        var second = await provider.GetBearerTokenAsync(CancellationToken.None);

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
        using var provider = new AzureCredentialTypeSafeAuthorizationProvider(credential, timeProvider);

        await provider.GetBearerTokenAsync(CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromMinutes(55));
        var refreshed = await provider.GetBearerTokenAsync(CancellationToken.None);

        await Assert.That(refreshed).IsEqualTo("token-2");
        await Assert.That(credential.RequestCount).IsEqualTo(2);
    }

    [Test]
    [Arguments(null, typeof(ApiKeyTypeSafeAuthorizationProvider))]
    [Arguments("ApiKey", typeof(ApiKeyTypeSafeAuthorizationProvider))]
    [Arguments("azurecredential", typeof(AzureCredentialTypeSafeAuthorizationProvider))]
    public async Task GivenAuthMode_WhenRegistering_ThenUsesMatchingAuthorizationProvider(
        string? authMode,
        Type expectedProviderType)
    {
        using var host = CreateHost(authMode).Build();

        var provider = host.Services.GetRequiredService<ITypeSafeAuthorizationProvider>();

        await Assert.That(provider.GetType()).IsEqualTo(expectedProviderType);
    }

    [Test]
    public async Task GivenUnsupportedAuthMode_WhenRegistering_ThenRejectsConfiguration()
    {
        await Assert.That(() => CreateHost("ManagedIdentity")).Throws<OptionsValidationException>();
    }

    private static HostApplicationBuilder CreateHost(string? authMode)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["TYPESAFE_AUTH_MODE"] = authMode });
        builder.AddTypeSafeConfiguration();
        return builder;
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
