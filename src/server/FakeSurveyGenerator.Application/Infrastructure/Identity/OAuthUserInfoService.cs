using System.IdentityModel.Tokens.Jwt;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Shared.Caching;
using FakeSurveyGenerator.Application.Shared.Identity;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace FakeSurveyGenerator.Application.Infrastructure.Identity;

internal sealed class OAuthUserInfoService(
    HttpClient client,
    ILogger<OAuthUserInfoService> logger,
    IConfiguration configuration,
    ICache<OAuthUser> cache,
    ITokenProviderService tokenProviderService)
    : IUserService
{
    private readonly ICache<OAuthUser> _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly HttpClient _client = client ?? throw new ArgumentNullException(nameof(client));

    private readonly IConfiguration _configuration =
        configuration ?? throw new ArgumentNullException(nameof(configuration));

    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly ITokenProviderService _tokenProviderService =
        tokenProviderService ?? throw new ArgumentNullException(nameof(tokenProviderService));

    public string GetUserIdentity()
    {
        var accessToken = _tokenProviderService.GetToken();

        return new JwtSecurityToken(accessToken).Subject;
    }

    public async Task<IUser> GetUserInfo(CancellationToken cancellationToken)
    {
        var cacheKey = GetUserIdentity();

        var accessToken = _tokenProviderService.GetToken();

        return await _cache.GetOrCreateAsync(cacheKey, async token =>
        {
            var (_, isFailure, userInfo, error) = await GetUserInfoFromIdentityProvider(accessToken, token);

            if (isFailure)
                throw new InvalidOperationException($"Failed to get user info from Identity Provider: {error}");

            var id = userInfo.Claims.First(claim => claim.Type == "sub").Value;
            var name = userInfo.Claims.First(claim => claim.Type == "name").Value;
            var email = userInfo.Claims.First(claim => claim.Type == "email").Value;

            return new OAuthUser(id, name, email);
        }, cancellationToken: cancellationToken);
    }

    private async Task<Result<UserInfoResponse>> GetUserInfoFromIdentityProvider(string? accessToken,
        CancellationToken cancellationToken)
    {
        var identityProviderUrl = _configuration.GetValue<string>("IDENTITY_PROVIDER_URL") ??
                                  throw new InvalidOperationException("IDENTITY_PROVIDER_URL not found in config");

        try
        {
            _client.BaseAddress = new Uri(identityProviderUrl);

            var disco = await _client.GetDiscoveryDocumentAsync(identityProviderUrl, cancellationToken);

            if (disco.IsError)
                return Result.Failure<UserInfoResponse>($"Discovery document error: {disco.Error}");

            var userInfoResponse = await _client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = disco.UserInfoEndpoint,
                Token = accessToken
            }, cancellationToken);

            return Result.Success(userInfoResponse);
        }
        catch (TimeoutRejectedException e)
        {
            _logger.LogError(e,
                "The request to get user info from Identity Provider {IdentityProviderUrl} timed out",
                identityProviderUrl);
            return Result.Failure<UserInfoResponse>("Request Timed Out");
        }
        catch (BrokenCircuitException e)
        {
            _logger.LogError(e, "The circuit to {IdentityProviderUrl} is now broken and is not allowing calls",
                identityProviderUrl);
            return Result.Failure<UserInfoResponse>("Circuit Broken");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred with the request to {IdentityProviderUrl}",
                identityProviderUrl);
            return Result.Failure<UserInfoResponse>($"Unknown Error: {e.Message}");
        }
    }
}