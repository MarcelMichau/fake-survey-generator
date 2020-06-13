using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Caching;
using FakeSurveyGenerator.Application.Common.Identity;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace FakeSurveyGenerator.Infrastructure.Identity
{
    internal sealed class OAuthUserInfoService : IUserService
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ICache<OAuthUser> _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OAuthUserInfoService(HttpClient client, ILogger<OAuthUserInfoService> logger,
            IConfiguration configuration, ICache<OAuthUser> cache, IHttpContextAccessor httpContextAccessor)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string GetUserIdentity(CancellationToken cancellationToken)
        {
            var accessToken = GetToken();
            return new JwtSecurityToken(accessToken).Subject;
        }

        public async Task<IUser> GetUserInfo(CancellationToken cancellationToken)
        {
            var accessToken = GetToken();

            var cacheKey = $"{new JwtSecurityToken(accessToken).Subject}";

            var (isCached, cachedUserInfo) = await _cache.TryGetValueAsync(cacheKey, cancellationToken);

            if (isCached)
                return cachedUserInfo;

            var (_, isFailure, value) = await GetUserInfoFromIdentityProvider(accessToken, cancellationToken);

            if (isFailure)
                return new UnidentifiedUser();

            var id = value.Claims.First(claim => claim.Type == "sub").Value;
            var name = value.Claims.First(claim => claim.Type == "name").Value;
            var email = value.Claims.First(claim => claim.Type == "email").Value;

            var userInfo = new OAuthUser(id, name, email);

            await _cache.SetAsync(cacheKey, userInfo, 60, cancellationToken);

            return userInfo;
        }

        private string GetToken()
        {
            var accessToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Substring(7);

            return accessToken;
        }

        private async Task<Result<UserInfoResponse>> GetUserInfoFromIdentityProvider(string accessToken,
            CancellationToken cancellationToken)
        {
            var identityProviderUrl = _configuration.GetValue<string>("IDENTITY_PROVIDER_URL");

            try
            {
                _client.BaseAddress = new Uri(identityProviderUrl);

                var disco = await _client.GetDiscoveryDocumentAsync(identityProviderUrl, cancellationToken);

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
                return Result.Failure<UserInfoResponse>("Unknown Error");
            }
        }
    }
}