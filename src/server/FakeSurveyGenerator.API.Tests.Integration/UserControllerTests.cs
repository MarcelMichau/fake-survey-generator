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
using FakeSurveyGenerator.Application.Users.Queries.IsUserRegistered;
using FakeSurveyGenerator.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    [Collection(nameof(IntegrationTestFixture))]
    public sealed class UserControllerTests
    {
        private readonly HttpClient _existingUserClient;
        private readonly IntegrationTestWebApplicationFactory<Startup> _clientFactory;

        private readonly IUser _newTestUser =
            new TestUser("brand-new-test-id", "Brand New Test User", "brandnewtestuser@test.com");

        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public UserControllerTests(IntegrationTestFixture fixture)
        {
            _existingUserClient = fixture.Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(ConfigureAuthenticationHandler);
            }).CreateDefaultClient(new UnwrappingResponseHandler());

            _clientFactory = fixture.Factory;
        }

        [Fact]
        public async Task GivenExistingUserId_WhenCallingGetUser_ThenExistingUserShouldBeReturned()
        {
            const int userId = 1;

            const string expectedDisplayName = "Test User";
            const string expectedEmailAddress = "test.user@test.com";
            const string expectedExternalUserId = "test-id";

            var user = await _existingUserClient.GetFromJsonAsync<UserModel>($"api/user/{userId}");

            user.Id.Should().Be(userId);
            user.DisplayName.Should().Be(expectedDisplayName);
            user.EmailAddress.Should().Be(expectedEmailAddress);
            user.ExternalUserId.Should().Be(expectedExternalUserId);
        }

        [Fact]
        public async Task GivenExistingRegisteredUser_WhenCallingIsUserRegistered_ThenResponseShouldBeTrue()
        {
            const string userId = "test-id";

            var result = await _existingUserClient.GetFromJsonAsync<UserRegistrationStatusModel>($"api/user/isRegistered?userId={userId}");

            result.IsUserRegistered.Should().BeTrue();
        }

        [Fact]
        public async Task GivenNewUser_WhenCallingIsUserRegistered_ThenResponseShouldBeFalse()
        {
            const string userId = "non-existent-id";

            var result = await _existingUserClient.GetFromJsonAsync<UserRegistrationStatusModel>($"api/user/isRegistered?userId={userId}");

            result.IsUserRegistered.Should().BeFalse();
        }

        [Fact]
        public async Task GivenAuthenticatedNewUser_WhenCallingRegisterUser_ThenSuccessfulResponseWithNewlyRegisteredUserShouldBeReturned()
        {
            var newUserClient = _clientFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(ConfigureAuthenticationHandler)
                    .ConfigureServices(ConfigureNewUserUserService);
            }).CreateDefaultClient(new UnwrappingResponseHandler());

            var registerUserCommand = new RegisterUserCommand();

            var response = await newUserClient.PostAsJsonAsync("/api/user/register", registerUserCommand, Options);

            response.EnsureSuccessStatusCode();

            await using var content = await response.Content.ReadAsStreamAsync();

            var user = await JsonSerializer.DeserializeAsync<UserModel>(content, Options);

            user.Id.Should().BePositive();
            user.DisplayName.Should().Be(_newTestUser.DisplayName);
            user.EmailAddress.Should().Be(_newTestUser.EmailAddress);
            user.ExternalUserId.Should().Be(_newTestUser.Id);
        }

        [Fact]
        public async Task GivenExistingUser_WhenCallingRegisterUser_ThenBadRequestResponseShouldBeReturned()
        {
            var registerUserCommand = new RegisterUserCommand();

            var response = await _existingUserClient.PostAsJsonAsync("/api/user/register", registerUserCommand, Options);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private static void ConfigureAuthenticationHandler(IServiceCollection services)
        {
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "Test", _ => { });
        }

        private void ConfigureNewUserUserService(IServiceCollection services)
        {
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(service => service.GetUserInfo(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_newTestUser);
            mockUserService.Setup(service => service.GetUserIdentity())
                .Returns(_newTestUser.Id);

            services.AddScoped(_ => mockUserService.Object);
        }
    }
}