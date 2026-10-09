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

public sealed class GetUserQueryTests
{
    [ClassDataSource<TestFixture>]
    public required TestFixture Fixture { get; init; }
    private readonly IValidator<GetUserQuery> _mockValidator = Substitute.For<IValidator<GetUserQuery>>();
    private readonly IUserService _mockUserService = Substitute.For<IUserService>();

    public GetUserQueryTests()
    {
        _mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(TestUser.Instance);

        // Setup mock validator to always return successful validation
        _mockValidator.ValidateAsync(Arg.Any<GetUserQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    [Test]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenExpectedResultTypeShouldBeReturned()
    {
        const int id = 1;

        var query = new GetUserQuery(id);

        var handler = new GetUserQueryHandler(Fixture.Context, _mockUserService, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That((object)result).IsTypeOf<Result<UserModel, Error>>();
    }

    [Test]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenReturnedUserIdShouldMatchGivenUserId()
    {
        const int id = 1;

        var query = new GetUserQuery(id);

        var handler = new GetUserQueryHandler(Fixture.Context, _mockUserService, _mockValidator);

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

        var handler = new GetUserQueryHandler(Fixture.Context, _mockUserService, _mockValidator);

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

        var handler = new GetUserQueryHandler(Fixture.Context, _mockUserService, _mockValidator);

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

        var handler = new GetUserQueryHandler(Fixture.Context, _mockUserService, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        var user = result.Value;

        await Assert.That(user.ExternalUserId).IsEqualTo(expectedExternalUserId);
    }

    [Test]
    public async Task GivenUserIdOfAnotherUser_WhenCallingHandle_ThenForbiddenErrorShouldBeReturnedWithoutUserDetails()
    {
        var otherUserService = Substitute.For<IUserService>();
        otherUserService.GetUserInfo(Arg.Any<CancellationToken>())
            .Returns(new TestUser("someone-else", "Someone Else", "someone.else@test.com"));

        var handler = new GetUserQueryHandler(Fixture.Context, otherUserService, _mockValidator);

        var result = await handler.Handle(new GetUserQuery(1), CancellationToken.None);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(Errors.General.Forbidden());
    }

    [Test]
    public async Task GivenUserIdWhichDoesNotExist_WhenCallingHandle_ThenNotFoundErrorShouldBeReturned()
    {
        var handler = new GetUserQueryHandler(Fixture.Context, _mockUserService, _mockValidator);

        var result = await handler.Handle(new GetUserQuery(int.MaxValue), CancellationToken.None);

        await Assert.That(result.Error).IsEqualTo(Errors.General.NotFound());
    }
}
