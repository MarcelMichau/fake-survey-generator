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

            if (httpContext is null)
                throw new InvalidOperationException("Tried to get a JWT from outside an HttpContext");

            if (httpContext.User.Identity is {IsAuthenticated: false})
                throw new InvalidOperationException("Cannot retrieve a token for an unauthorized user");

            var accessToken = httpContext.Request.Headers[HeaderNames.Authorization].ToString().Substring(7);

            return accessToken;
        }
    }
}
