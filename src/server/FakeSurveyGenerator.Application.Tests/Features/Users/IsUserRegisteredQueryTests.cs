using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.Shared.Errors;
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

    public IsUserRegisteredQueryTests()
    {
        // Setup mock validator to always return successful validation
        _mockValidator.ValidateAsync(Arg.Any<IsUserRegisteredQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    [Test]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenExpectedResultTypeShouldBeReturned()
    {
        const string userId = "test-id";

        var query = new IsUserRegisteredQuery(userId);

        var handler = new IsUserRegisteredQueryHandler(Fixture.Context, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That((object)result).IsTypeOf<Result<UserRegistrationStatusModel, Error>>();
    }

    [Test]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenReturnedResultShouldBeTrue()
    {
        const string userId = "test-id";

        var query = new IsUserRegisteredQuery(userId);

        var handler = new IsUserRegisteredQueryHandler(Fixture.Context, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That(result.Value.IsUserRegistered).IsTrue();
    }

    [Test]
    public async Task GivenNewUserId_WhenCallingHandle_ThenReturnedResultShouldBeFalse()
    {
        const string userId = "unregistered-id";

        var query = new IsUserRegisteredQuery(userId);

        var handler = new IsUserRegisteredQueryHandler(Fixture.Context, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That(result.Value.IsUserRegistered).IsFalse();
    }
}