using System.Net.Http;
using System.Threading;
using AutoWrapper.Server;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    public static class WebApplicationFactoryExtensions
    {
        public static HttpClient WithSpecificUser(this IntegrationTestWebApplicationFactory<Startup> factory, string id, string displayName, string emailAddress)
        {
            var user = new TestUser(id, displayName, emailAddress);

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
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(testUser);
            mockUserService.Setup(service => service.GetUserIdentity())
                .Returns(testUser.Id);

            services.AddScoped(_ => mockUserService.Object);
        }
    }
}