using FakeSurveyGenerator.Application.Shared.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace FakeSurveyGenerator.Api.Tests.Integration.Setup;

public static class WebApplicationFactoryExtensions
{
    public static HttpClient WithSpecificUser(this WebApplicationFactory<Program>? factory, IUser user)
    {
        return factory!.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(ConfigureAuthenticationHandler)
                .ConfigureServices(services => ConfigureNewUserUserService(services, user));
        }).CreateDefaultClient();
    }

    private static void ConfigureAuthenticationHandler(IServiceCollection services)
    {
        const string defaultScheme = "TestScheme";
        services.AddAuthentication(defaultScheme)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                defaultScheme, _ => { });
    }

    private static void ConfigureNewUserUserService(IServiceCollection services, IUser testUser)
    {
        var mockUserService = Substitute.For<IUserService>();
        mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(testUser);
        mockUserService.GetUserIdentity().Returns(testUser.Id);

        services.AddScoped(_ => mockUserService);
    }
}