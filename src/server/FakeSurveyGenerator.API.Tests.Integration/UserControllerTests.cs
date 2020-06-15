using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Server;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Users.Commands.RegisterUser;
using FakeSurveyGenerator.Application.Users.Models;
using FakeSurveyGenerator.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    public sealed class UserControllerTests : IClassFixture<IntegrationTestWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestWebApplicationFactory<Startup> _clientFactory;

        private readonly IUser _newTestUser =
            new TestUser("brand-new-test-id", "Brand New Test User", "brandnewtestuser@test.com");

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public UserControllerTests(IntegrationTestWebApplicationFactory<Startup> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(ConfigureAuthenticationHandler)
                    .ConfigureServices(ConfigureNewUserUserService);
            }).CreateDefaultClient(new UnwrappingResponseHandler());

            _clientFactory = factory;
        }

        [Fact]
        public async Task GetUser_Should_Return_User()
        {
            const int userId = 1;

            const string expectedDisplayName = "Test User";
            const string expectedEmailAddress = "test.user@test.com";
            const string expectedExternalUserId = "test-id";

            var user = await _client.GetFromJsonAsync<UserModel>($"api/user/{userId}");

            user.Id.ShouldBe(userId);
            user.DisplayName.ShouldBe(expectedDisplayName);
            user.EmailAddress.ShouldBe(expectedEmailAddress);
            user.ExternalUserId.ShouldBe(expectedExternalUserId);
        }

        [Fact]
        public async Task IsUserRegistered_Should_Return_True_For_Existing_User()
        {
            const string userId = "test-id";

            var result = await _client.GetFromJsonAsync<bool>($"api/user/isRegistered?userId={userId}");

            result.ShouldBeTrue();
        }

        [Fact]
        public async Task IsUserRegistered_Should_Return_False_For_NonExistent_User()
        {
            const string userId = "non-existent-id";

            var result = await _client.GetFromJsonAsync<bool>($"api/user/isRegistered?userId={userId}");

            result.ShouldBeFalse();
        }

        [Fact]
        public async Task RegisterUser_Should_Create_User()
        {
            var clientForNewUser = _clientFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(ConfigureAuthenticationHandler)
                    .ConfigureServices(ConfigureNewUserUserService);
            }).CreateDefaultClient(new UnwrappingResponseHandler());

            var registerUserCommand = new RegisterUserCommand();

            var response = await clientForNewUser.PostAsJsonAsync("/api/user/register", registerUserCommand, Options);

            response.EnsureSuccessStatusCode();

            await using var content = await response.Content.ReadAsStreamAsync();

            var user = await JsonSerializer.DeserializeAsync<UserModel>(content, Options);

            user.Id.ShouldBeGreaterThan(0);
            user.DisplayName.ShouldBe(_newTestUser.DisplayName);
            user.EmailAddress.ShouldBe(_newTestUser.EmailAddress);
            user.ExternalUserId.ShouldBe(_newTestUser.Id);
        }

        [Fact]
        public async Task RegisterUser_Should_Return_BadRequest_When_Trying_To_Register_Existing_User()
        {
            var registerUserCommand = new RegisterUserCommand();

            var response = await _client.PostAsJsonAsync("/api/user/register", registerUserCommand, Options);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        private static void ConfigureAuthenticationHandler(IServiceCollection services)
        {
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "Test", options => { });
        }

        private void ConfigureNewUserUserService(IServiceCollection services)
        {
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_newTestUser);
            mockUserService.Setup(service => service.GetUserIdentity())
                .Returns(_newTestUser.Id);

            services.AddScoped(sp => mockUserService.Object);
        }
    }
}