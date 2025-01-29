using System.Net;
using System.Net.Http.Json;
using AutoFixture;
using FakeSurveyGenerator.Api.Tests.Integration.Setup;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.TestHelpers;

namespace FakeSurveyGenerator.Api.Tests.Integration.Users;

public sealed class UserEndpointsTests
{
    [ClassDataSource<IntegrationTestFixture>(Shared = SharedType.PerTestSession)]
    public required IntegrationTestFixture TestFixture { get; init; }
    private readonly IFixture _fixture = new Fixture();

    [Test]
    public async Task GivenExistingUserId_WhenCallingGetUser_ThenExistingUserShouldBeReturned()
    {
        var client = TestFixture.Factory.WithSpecificUser(_fixture.Create<TestUser>());

        var newUser = await RegisterNewUser(client);

        var user = await client.GetFromJsonAsync<UserModel>($"api/user/{newUser.Id}");

        await Assert.That(user!.Id).IsEqualTo(newUser.Id);
        await Assert.That(user.DisplayName).IsEqualTo(newUser.DisplayName);
        await Assert.That(user.EmailAddress).IsEqualTo(newUser.EmailAddress);
        await Assert.That(user.ExternalUserId).IsEqualTo(newUser.ExternalUserId);
    }

    [Test]
    public async Task GivenExistingRegisteredUser_WhenCallingIsUserRegistered_ThenResponseShouldBeTrue()
    {
        var client = TestFixture.Factory.WithSpecificUser(_fixture.Create<TestUser>());

        var newUser = await RegisterNewUser(client);

        var result =
            await client.GetFromJsonAsync<UserRegistrationStatusModel>(
                $"api/user/isRegistered?userId={newUser.ExternalUserId}");

        await Assert.That(result!.IsUserRegistered).IsTrue();
    }

    [Test]
    public async Task GivenNewUser_WhenCallingIsUserRegistered_ThenResponseShouldBeFalse()
    {
        const string userId = "non-existent-id";

        var client = TestFixture.Factory.WithSpecificUser(_fixture.Create<TestUser>());

        var result =
            await client.GetFromJsonAsync<UserRegistrationStatusModel>($"api/user/isRegistered?userId={userId}");

        await Assert.That(result!.IsUserRegistered).IsFalse();
    }

    [Test]
    public async Task
        GivenAuthenticatedNewUser_WhenCallingRegisterUser_ThenSuccessfulResponseWithNewlyRegisteredUserShouldBeReturned()
    {
        var expectedUser = _fixture.Create<TestUser>();

        var client = TestFixture.Factory.WithSpecificUser(expectedUser);

        var user = await RegisterNewUser(client);

        await Assert.That(user.Id).IsPositive();
        await Assert.That(user.DisplayName).IsEqualTo(expectedUser.DisplayName);
        await Assert.That(user.EmailAddress).IsEqualTo(expectedUser.EmailAddress);
        await Assert.That(user.ExternalUserId).IsEqualTo(expectedUser.Id);
    }

    [Test]
    public async Task GivenExistingUser_WhenCallingRegisterUser_ThenBadRequestResponseShouldBeReturned()
    {
        var client = TestFixture.Factory.WithSpecificUser(_fixture.Create<TestUser>());

        var _ = await RegisterNewUser(client);
        var registerUserCommand = new RegisterUserCommand();

        var response = await client.PostAsJsonAsync("/api/user/register", registerUserCommand);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    private static async Task<UserModel> RegisterNewUser(HttpClient client)
    {
        var registerUserCommand = new RegisterUserCommand();

        var response = await client.PostAsJsonAsync("/api/user/register", registerUserCommand);

        response.EnsureSuccessStatusCode();

        var user = await response.Content.ReadFromJsonAsync<UserModel>();
        return user!;
    }
}