using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace FakeSurveyGenerator.Infrastructure.Identity
{
    internal sealed class JwtBearerTokenProviderService : ITokenProviderService
    {
        private readonly IServiceProvider _serviceProvider;

        public JwtBearerTokenProviderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public string GetToken()
        {
            var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;

            if (httpContext.User.Identity != null && !httpContext.User.Identity.IsAuthenticated)
                throw new InvalidOperationException("Cannot retrieve a token for an unauthorized user");

            var accessToken = httpContext.Request.Headers[HeaderNames.Authorization].ToString().Substring(7);

            return accessToken;
        }
    }
}
