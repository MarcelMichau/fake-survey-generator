using FakeSurveyGenerator.Application.Domain.Shared;
using FakeSurveyGenerator.Application.Domain.Users;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Shared.Caching;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Shared.Identity;
using FakeSurveyGenerator.Application.TestHelpers;
using FakeSurveyGenerator.Application.Tests.Setup;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace FakeSurveyGenerator.Application.Tests.Features.Surveys;

[NotInParallel]
public sealed class DeleteSurveyCommandTests
{
    [ClassDataSource<TestFixture>]
    public required TestFixture Fixture { get; init; }

    private static ICache<SurveyModel?> Cache => TestFixture.GetCache<SurveyModel?>();
    private readonly IUserService _mockUserService = Substitute.For<IUserService>();
    private readonly IValidator<DeleteSurveyCommand> _mockValidator = Substitute.For<IValidator<DeleteSurveyCommand>>();

    public DeleteSurveyCommandTests()
    {
        _mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(TestUser.Instance);

        _mockValidator.ValidateAsync(Arg.Any<DeleteSurveyCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    [Test]
    public async Task GivenOwnerDeletingExistingSurvey_WhenCallingHandle_ThenSurveyShouldBeRemovedFromDatabase()
    {
        const int id = 1;

        var sut = new DeleteSurveyCommandHandler(Fixture.Context, _mockUserService, Cache, _mockValidator);

        var result = await sut.Handle(new DeleteSurveyCommand(id), CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value).IsEqualTo(id);

        var deletedSurvey = await Fixture.Context.Surveys.FindAsync(id);
        await Assert.That(deletedSurvey).IsNull();
    }

    [Test]
    public async Task GivenNonOwnerAttemptingDelete_WhenCallingHandle_ThenForbiddenErrorShouldBeReturned()
    {
        const int id = 2;

        var otherUser = new User(NonEmptyString.Create("Other User"),
            NonEmptyString.Create("other.user@test.com"),
            NonEmptyString.Create("other-external-id"));
        await Fixture.Context.Users.AddAsync(otherUser);
        await Fixture.Context.SaveChangesAsync();

        var nonOwnerUserService = Substitute.For<IUserService>();
        nonOwnerUserService.GetUserInfo(Arg.Any<CancellationToken>())
            .Returns(new TestUser("other-external-id", "Other User", "other.user@test.com"));

        var sut = new DeleteSurveyCommandHandler(Fixture.Context, nonOwnerUserService, Cache, _mockValidator);

        var result = await sut.Handle(new DeleteSurveyCommand(id), CancellationToken.None);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(Errors.General.Forbidden());

        var survey = await Fixture.Context.Surveys.FindAsync(id);
        await Assert.That(survey).IsNotNull();
    }

    [Test]
    public async Task GivenNonExistentSurveyId_WhenCallingHandle_ThenNotFoundErrorShouldBeReturned()
    {
        const int id = 999;

        var sut = new DeleteSurveyCommandHandler(Fixture.Context, _mockUserService, Cache, _mockValidator);

        var result = await sut.Handle(new DeleteSurveyCommand(id), CancellationToken.None);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(Errors.General.NotFound());
    }

    [Test]
    public async Task GivenOwnerDeletingExistingSurvey_WhenCallingHandle_ThenCacheShouldBeInvalidatedForThatSurveyId()
    {
        const int id = 3;

        var cache = Cache;

        var sut = new DeleteSurveyCommandHandler(Fixture.Context, _mockUserService, cache, _mockValidator);

        var result = await sut.Handle(new DeleteSurveyCommand(id), CancellationToken.None);

        await Assert.That(result.IsSuccess).IsTrue();

        await cache.Received(1).RemoveAsync(id.ToString(), Arg.Any<CancellationToken>());
    }
}
