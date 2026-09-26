using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using FakeSurveyGenerator.Application.Infrastructure.Identity;
using FakeSurveyGenerator.Application.Tests.Setup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FakeSurveyGenerator.Application.Tests.Infrastructure.Identity;

public sealed class OAuthUserInfoServiceTests
{
    [Test]
    public async Task GetUserInfo_WhenProviderReturnsUserClaims_MapsTheClaimsToIUser()
    {
        const string subject = "subject-123";
        const string displayName = "Survey User";
        const string email = "survey.user@example.test";
        var token = CreateAccessToken(subject);
        var requests = new List<HttpRequestMessage>();
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            requests.Add(request);
            var responseBody = request.RequestUri!.AbsolutePath.EndsWith("openid-configuration", StringComparison.Ordinal)
                ? CreateDiscoveryDocument()
                : $$"""{"sub":"{{subject}}","name":"{{displayName}}","email":"{{email}}"}""";

            return Task.FromResult(JsonResponse(HttpStatusCode.OK, responseBody));
        });
        var tokenProvider = Substitute.For<ITokenProviderService>();
        tokenProvider.GetToken().Returns(token);
        var service = CreateService(handler, tokenProvider);

        var user = await service.GetUserInfo(CancellationToken.None);

        await Assert.That(user.Id).IsEqualTo(subject);
        await Assert.That(user.DisplayName).IsEqualTo(displayName);
        await Assert.That(user.EmailAddress).IsEqualTo(email);
        await Assert.That(requests.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(requests[^1].Headers.Authorization!.Scheme).IsEqualTo("Bearer");
        await Assert.That(requests[^1].Headers.Authorization!.Parameter).IsEqualTo(token);
    }

    [Test]
    public async Task GetUserInfo_WhenDiscoveryDocumentRequestFails_ThrowsProviderFailure()
    {
        var tokenProvider = Substitute.For<ITokenProviderService>();
        tokenProvider.GetToken().Returns(CreateAccessToken("subject-123"));
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(JsonResponse(HttpStatusCode.ServiceUnavailable, "identity provider unavailable")));
        var service = CreateService(handler, tokenProvider);

        await Assert.That(async () => await service.GetUserInfo(CancellationToken.None))
            .ThrowsException().And.IsTypeOf<InvalidOperationException>();
    }

    private static OAuthUserInfoService CreateService(
        HttpMessageHandler handler,
        ITokenProviderService tokenProvider)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IDENTITY_PROVIDER_URL"] = "https://identity.test"
            })
            .Build();

        return new OAuthUserInfoService(
            new HttpClient(handler),
            NullLogger<OAuthUserInfoService>.Instance,
            configuration,
            TestFixture.GetHybridCache(),
            tokenProvider);
    }

    private static string CreateAccessToken(string subject)
    {
        var jwt = new JwtSecurityToken(
            issuer: "https://identity.test",
            claims: [new Claim(JwtRegisteredClaimNames.Sub, subject)]);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private static string CreateDiscoveryDocument()
    {
        return """
               {
                 "issuer": "https://identity.test",
                 "jwks_uri": "https://identity.test/.well-known/jwks.json",
                 "authorization_endpoint": "https://identity.test/connect/authorize",
                 "token_endpoint": "https://identity.test/connect/token",
                 "userinfo_endpoint": "https://identity.test/connect/userinfo",
                 "response_types_supported": ["code"],
                 "subject_types_supported": ["public"],
                 "id_token_signing_alg_values_supported": ["RS256"]
               }
               """;
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string content)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return sendAsync(request, cancellationToken);
        }
    }
}
