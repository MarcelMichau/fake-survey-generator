using System.Net.Http;
using System.Threading;
using AutoWrapper.Server;
using FakeSurveyGenerator.Application.Common.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    public static class WebApplicationFactoryExtensions
    {
        public static HttpClient WithSpecificUser(this IntegrationTestWebApplicationFactory<Startup> factory, IUser user)
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(ConfigureAuthenticationHandler)
                    .ConfigureServices(services => ConfigureNewUserUserService(services, user));
            }).CreateDefaultClient(new UnwrappingResponseHandler());
        }

        private static void ConfigureAuthenticationHandler(IServiceCollection services)
        {
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "Test", _ => { });
        }

        private static void ConfigureNewUserUserService(IServiceCollection services, IUser testUser)
        {
            var mockUserService = Substitute.For<IUserService>();
            mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(testUser);
            mockUserService.GetUserIdentity().Returns(testUser.Id);

            services.AddScoped(_ => mockUserService);
        }
    }
}