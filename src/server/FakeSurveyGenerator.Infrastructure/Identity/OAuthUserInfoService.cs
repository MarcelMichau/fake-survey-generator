using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Identity;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Infrastructure.Identity
{
    internal sealed class OAuthUserInfoService : IUserService
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public OAuthUserInfoService(HttpClient client, ILogger<OAuthUserInfoService> logger, IConfiguration configuration)
        {
            _client = client;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IUser> GetUserInfo(string accessToken, CancellationToken cancellationToken)
        {
            var identityProviderUrl = _configuration.GetValue<string>("IDENTITY_PROVIDER_URL");

            _client.BaseAddress = new Uri(identityProviderUrl);

            var disco = await _client.GetDiscoveryDocumentAsync(identityProviderUrl, cancellationToken);

            var userInfo = await _client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = disco.UserInfoEndpoint,
                Token = accessToken
            }, cancellationToken);

            var id = userInfo.Claims.First(claim => claim.Type == "sub").Value;
            var name = userInfo.Claims.First(claim => claim.Type == "name").Value;
            var email = userInfo.Claims.First(claim => claim.Type == "email").Value;

            return new OAuthUser(id, name, email);
        }
    }
}
