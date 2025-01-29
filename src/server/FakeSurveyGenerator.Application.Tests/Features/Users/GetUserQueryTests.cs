using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Tests.Setup;

namespace FakeSurveyGenerator.Application.Tests.Features.Users;

public sealed class GetUserQueryTests
{
    [ClassDataSource<TestFixture>]
    public required TestFixture Fixture { get; init; }

    [Test]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenExpectedResultTypeShouldBeReturned()
    {
        const int id = 1;

        var query = new GetUserQuery(id);

        var handler = new GetUserQueryHandler(Fixture.Context);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That((object)result).IsTypeOf<Result<UserModel, Error>>();
    }

    [Test]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenReturnedUserIdShouldMatchGivenUserId()
    {
        const int id = 1;

        var query = new GetUserQuery(id);

        var handler = new GetUserQueryHandler(Fixture.Context);

        var result = await handler.Handle(query, CancellationToken.None);

        var user = result.Value;

        await Assert.That(user.Id).IsEqualTo(id);
    }

    [Test]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenReturnedDisplayNameShouldMatchExpectedValue()
    {
        const int id = 1;
        const string expectedDisplayName = "Test User";

        var query = new GetUserQuery(id);

        var handler = new GetUserQueryHandler(Fixture.Context);

        var result = await handler.Handle(query, CancellationToken.None);

        var user = result.Value;

        await Assert.That(user.DisplayName).IsEqualTo(expectedDisplayName);
    }

    [Test]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenReturnedEmailAddressShouldMatchExpectedValue()
    {
        const int id = 1;
        const string expectedEmailAddress = "test.user@test.com";

        var query = new GetUserQuery(id);

        var handler = new GetUserQueryHandler(Fixture.Context);

        var result = await handler.Handle(query, CancellationToken.None);

        var user = result.Value;

        await Assert.That(user.EmailAddress).IsEqualTo(expectedEmailAddress);
    }

    [Test]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenReturnedExternalUserIdShouldMatchExpectedValue()
    {
        const int id = 1;
        const string expectedExternalUserId = "test-id";

        var query = new GetUserQuery(id);

        var handler = new GetUserQueryHandler(Fixture.Context);

        var result = await handler.Handle(query, CancellationToken.None);

        var user = result.Value;

        await Assert.That(user.ExternalUserId).IsEqualTo(expectedExternalUserId);
    }
}
