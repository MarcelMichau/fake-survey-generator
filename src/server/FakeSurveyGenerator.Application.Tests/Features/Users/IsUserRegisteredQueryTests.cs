using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Shared.Identity;
using FakeSurveyGenerator.Application.TestHelpers;
using FakeSurveyGenerator.Application.Tests.Setup;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace FakeSurveyGenerator.Application.Tests.Features.Users;

public sealed class IsUserRegisteredQueryTests
{
    [ClassDataSource<TestFixture>]
    public required TestFixture Fixture { get; init; }
    private readonly IValidator<IsUserRegisteredQuery> _mockValidator = Substitute.For<IValidator<IsUserRegisteredQuery>>();
    private readonly IUserService _mockUserService = Substitute.For<IUserService>();

    public IsUserRegisteredQueryTests()
    {
        _mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(TestUser.Instance);

        // Setup mock validator to always return successful validation
        _mockValidator.ValidateAsync(Arg.Any<IsUserRegisteredQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    [Test]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenExpectedResultTypeShouldBeReturned()
    {
        const string userId = "test-id";

        var query = new IsUserRegisteredQuery(userId);

        var handler = new IsUserRegisteredQueryHandler(Fixture.Context, _mockUserService, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That((object)result).IsTypeOf<Result<UserRegistrationStatusModel, Error>>();
    }

    [Test]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenReturnedResultShouldBeTrue()
    {
        const string userId = "test-id";

        var query = new IsUserRegisteredQuery(userId);

        var handler = new IsUserRegisteredQueryHandler(Fixture.Context, _mockUserService, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That(result.Value.IsUserRegistered).IsTrue();
    }

    [Test]
    public async Task GivenNewUserId_WhenCallingHandle_ThenReturnedResultShouldBeFalse()
    {
        const string userId = "unregistered-id";

        var query = new IsUserRegisteredQuery(userId);

        var unregisteredUserService = Substitute.For<IUserService>();
        unregisteredUserService.GetUserInfo(Arg.Any<CancellationToken>())
            .Returns(new TestUser(userId, "Unregistered", "unregistered@test.com"));
        var handler = new IsUserRegisteredQueryHandler(Fixture.Context, unregisteredUserService, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That(result.Value.IsUserRegistered).IsFalse();
    }

    [Test]
    public async Task GivenUserIdOfAnotherUser_WhenCallingHandle_ThenForbiddenErrorShouldBeReturned()
    {
        var query = new IsUserRegisteredQuery("some-other-users-id");

        var handler = new IsUserRegisteredQueryHandler(Fixture.Context, _mockUserService, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(Errors.General.Forbidden());
    }
}
