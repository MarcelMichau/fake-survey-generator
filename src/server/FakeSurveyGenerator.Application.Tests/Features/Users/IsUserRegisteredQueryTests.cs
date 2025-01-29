using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.Tests.Setup;

namespace FakeSurveyGenerator.Application.Tests.Features.Users;

public sealed class IsUserRegisteredQueryTests
{
    [ClassDataSource<TestFixture>]
    public required TestFixture Fixture { get; init; }

    [Test]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenExpectedResultTypeShouldBeReturned()
    {
        const string userId = "test-id";

        var query = new IsUserRegisteredQuery(userId);

        var handler = new IsUserRegisteredQueryHandler(Fixture.Context);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That((object)result).IsTypeOf<Result<UserRegistrationStatusModel>>();
    }

    [Test]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenReturnedResultShouldBeTrue()
    {
        const string userId = "test-id";

        var query = new IsUserRegisteredQuery(userId);

        var handler = new IsUserRegisteredQueryHandler(Fixture.Context);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That(result.Value.IsUserRegistered).IsTrue();
    }

    [Test]
    public async Task GivenNewUserId_WhenCallingHandle_ThenReturnedResultShouldBeFalse()
    {
        const string userId = "unregistered-id";

        var query = new IsUserRegisteredQuery(userId);

        var handler = new IsUserRegisteredQueryHandler(Fixture.Context);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That(result.Value.IsUserRegistered).IsFalse();
    }
}