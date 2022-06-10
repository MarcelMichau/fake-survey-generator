using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using FakeSurveyGenerator.Application.Users.Commands.RegisterUser;
using FakeSurveyGenerator.Application.Users.Models;
using FakeSurveyGenerator.Application.Users.Queries.IsUserRegistered;
using FakeSurveyGenerator.Data;
using FluentAssertions;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration.Controllers;

[Collection(nameof(IntegrationTestFixture))]
public sealed class UserControllerTests
{
    private readonly IntegrationTestWebApplicationFactory _factory;
    private readonly IFixture _fixture;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public UserControllerTests(IntegrationTestFixture fixture)
    {
        _factory = fixture.Factory;
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GivenExistingUserId_WhenCallingGetUser_ThenExistingUserShouldBeReturned()
    {
        var client = _factory.WithSpecificUser(_fixture.Create<TestUser>());

        var newUser = await RegisterNewUser(client);

        var user = await client.GetFromJsonAsync<UserModel>($"api/user/{newUser.Id}");

        user.Id.Should().Be(newUser.Id);
        user.DisplayName.Should().Be(newUser.DisplayName);
        user.EmailAddress.Should().Be(newUser.EmailAddress);
        user.ExternalUserId.Should().Be(newUser.ExternalUserId);
    }

    [Fact]
    public async Task GivenExistingRegisteredUser_WhenCallingIsUserRegistered_ThenResponseShouldBeTrue()
    {
        var client = _factory.WithSpecificUser(_fixture.Create<TestUser>());

        var newUser = await RegisterNewUser(client);

        var result = await client.GetFromJsonAsync<UserRegistrationStatusModel>($"api/user/isRegistered?userId={newUser.ExternalUserId}");

        result.IsUserRegistered.Should().BeTrue();
    }

    [Fact]
    public async Task GivenNewUser_WhenCallingIsUserRegistered_ThenResponseShouldBeFalse()
    {
        const string userId = "non-existent-id";

        var client = _factory.WithSpecificUser(_fixture.Create<TestUser>());

        var result = await client.GetFromJsonAsync<UserRegistrationStatusModel>($"api/user/isRegistered?userId={userId}");

        result.IsUserRegistered.Should().BeFalse();
    }

    [Fact]
    public async Task GivenAuthenticatedNewUser_WhenCallingRegisterUser_ThenSuccessfulResponseWithNewlyRegisteredUserShouldBeReturned()
    {
        var expectedUser = _fixture.Create<TestUser>();

        var client = _factory.WithSpecificUser(expectedUser);

        var user = await RegisterNewUser(client);

        user.Id.Should().BePositive();
        user.DisplayName.Should().Be(expectedUser.DisplayName);
        user.EmailAddress.Should().Be(expectedUser.EmailAddress);
        user.ExternalUserId.Should().Be(expectedUser.Id);
    }

    [Fact]
    public async Task GivenExistingUser_WhenCallingRegisterUser_ThenBadRequestResponseShouldBeReturned()
    {
        var client = _factory.WithSpecificUser(_fixture.Create<TestUser>());

        var _ = await RegisterNewUser(client);
        var registerUserCommand = new RegisterUserCommand();

        var response = await client.PostAsJsonAsync("/api/user/register", registerUserCommand, Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static async Task<UserModel> RegisterNewUser(HttpClient client)
    {
        var registerUserCommand = new RegisterUserCommand();

        var response = await client.PostAsJsonAsync("/api/user/register", registerUserCommand, Options);

        response.EnsureSuccessStatusCode();

        await using var content = await response.Content.ReadAsStreamAsync();

        var user = await JsonSerializer.DeserializeAsync<UserModel>(content, Options);
        return user;
    }
}