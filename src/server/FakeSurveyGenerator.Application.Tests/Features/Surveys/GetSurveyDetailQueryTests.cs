using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Shared.Identity;
using FakeSurveyGenerator.Application.TestHelpers;
using FakeSurveyGenerator.Application.Tests.Setup;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Hybrid;
using NSubstitute;

namespace FakeSurveyGenerator.Application.Tests.Features.Surveys;

public sealed class GetSurveyDetailQueryTests
{
    [ClassDataSource<TestFixture>]
    public required TestFixture Fixture { get; init; }
    private static HybridCache Cache => TestFixture.GetHybridCache();
    private readonly IValidator<GetSurveyDetailQuery> _mockValidator = Substitute.For<IValidator<GetSurveyDetailQuery>>();
    private readonly IUserService _mockUserService = Substitute.For<IUserService>();

    public GetSurveyDetailQueryTests()
    {
        _mockUserService.GetUserInfo(Arg.Any<CancellationToken>()).Returns(TestUser.Instance);

        // Setup mock validator to always return successful validation
        _mockValidator.ValidateAsync(Arg.Any<GetSurveyDetailQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    [Test]
    public async Task GivenExistingSurveyId_WhenCallingHandle_ThenExpectedResultTypeShouldBeReturned()
    {
        const int id = 1;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(Fixture.Context, _mockUserService, Cache, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That((object)result).IsTypeOf<Result<SurveyModel, Error>>();
    }

    [Test]
    public async Task GivenExistingSurveyId_WhenCallingHandle_ThenReturnedSurveyIdShouldMatchGivenSurveyId()
    {
        const int id = 1;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(Fixture.Context, _mockUserService, Cache, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        var survey = result.Value;

        await Assert.That(survey.Id).IsEqualTo(id);
    }

    [Test]
    public async Task GivenExistingSurveyId_WhenCallingHandle_ThenReturnedSurveyTopicShouldMatchExpectedValue()
    {
        const int id = 1;
        const string expectedTopicText = "Test Topic 1";

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(Fixture.Context, _mockUserService, Cache, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        var survey = result.Value;

        await Assert.That(survey.Topic).IsEqualTo(expectedTopicText);
    }

    [Test]
    public async Task
        GivenExistingSurveyId_WhenCallingHandle_ThenReturnedSurveyNumberOfRespondentsShouldMatchExpectedValue()
    {
        const int id = 1;
        const int expectedNumberOfRespondents = 10;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(Fixture.Context, _mockUserService, Cache, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        var survey = result.Value;

        await Assert.That(survey.NumberOfRespondents).IsEqualTo(expectedNumberOfRespondents);
    }

    [Test]
    public async Task GivenExistingSurveyId_WhenCallingHandle_ThenReturnedSurveyRespondentTypeShouldMatchExpectedValue()
    {
        const int id = 1;
        const string expectedTopicText = "Testers";

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(Fixture.Context, _mockUserService, Cache, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        var survey = result.Value;

        await Assert.That(survey.RespondentType).IsEqualTo(expectedTopicText);
    }

    [Test]
    public async Task MapToModelAndSelectToModel_ReturnEquivalentSurveyModels()
    {
        const int id = 1;
        var entity = await Fixture.Context.Surveys
            .Include(survey => survey.Owner)
            .Include(survey => survey.Options)
            .SingleAsync(survey => survey.Id == id);

        var mapped = entity.MapToModel();
        var projected = await Fixture.Context.Surveys
            .SelectToModel()
            .SingleAsync(survey => survey.Id == id);

        await Assert.That(projected.Id).IsEqualTo(mapped.Id);
        await Assert.That(projected.OwnerId).IsEqualTo(mapped.OwnerId);
        await Assert.That(projected.OwnerExternalUserId).IsEqualTo(mapped.OwnerExternalUserId);
        await Assert.That(projected.Topic).IsEqualTo(mapped.Topic);
        await Assert.That(projected.RespondentType).IsEqualTo(mapped.RespondentType);
        await Assert.That(projected.NumberOfRespondents).IsEqualTo(mapped.NumberOfRespondents);
        await Assert.That(projected.IsRigged).IsEqualTo(mapped.IsRigged);
        await Assert.That(projected.CreatedBy).IsEqualTo(mapped.CreatedBy);
        await Assert.That(projected.CreatedOn).IsEqualTo(mapped.CreatedOn);
        await Assert.That(projected.ModifiedBy).IsEqualTo(mapped.ModifiedBy);
        await Assert.That(projected.ModifiedOn).IsEqualTo(mapped.ModifiedOn);
        await Assert.That(projected.Options.Count).IsEqualTo(mapped.Options.Count);

        for (var index = 0; index < mapped.Options.Count; index++)
        {
            await Assert.That(projected.Options[index].OptionText).IsEqualTo(mapped.Options[index].OptionText);
            await Assert.That(projected.Options[index].NumberOfVotes).IsEqualTo(mapped.Options[index].NumberOfVotes);
            await Assert.That(projected.Options[index].PreferredNumberOfVotes)
                .IsEqualTo(mapped.Options[index].PreferredNumberOfVotes);
        }
    }

    [Test]
    public async Task GivenSurveyIdWhichDoesNotExist_WhenCallingHandle_ThenResponseShouldIndicateNotFoundError()
    {
        const int id = 100;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(Fixture.Context, _mockUserService, Cache, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That(result.Error).IsEqualTo(Errors.General.NotFound());
    }

    [Test]
    public async Task GivenSurveyOwnedBySomeoneElse_WhenCallingHandle_ThenForbiddenErrorShouldBeReturned()
    {
        var otherUserService = Substitute.For<IUserService>();
        otherUserService.GetUserInfo(Arg.Any<CancellationToken>())
            .Returns(new TestUser("someone-else", "Someone Else", "someone.else@test.com"));

        var handler = new GetSurveyDetailQueryHandler(Fixture.Context, otherUserService, Cache, _mockValidator);

        var result = await handler.Handle(new GetSurveyDetailQuery(1), CancellationToken.None);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(Errors.General.Forbidden());
    }

    [Test]
    public async Task GivenSurveyOwnedBySomeoneElseAndAlreadyCached_WhenCallingHandle_ThenForbiddenErrorShouldBeReturned()
    {
        using var services = new ServiceCollection().AddHybridCache().Services.BuildServiceProvider();
        var cache = services.GetRequiredService<HybridCache>();

        var ownerHandler = new GetSurveyDetailQueryHandler(Fixture.Context, _mockUserService, cache, _mockValidator);
        var ownerResult = await ownerHandler.Handle(new GetSurveyDetailQuery(1), CancellationToken.None);

        var otherUserService = Substitute.For<IUserService>();
        otherUserService.GetUserInfo(Arg.Any<CancellationToken>())
            .Returns(new TestUser("someone-else", "Someone Else", "someone.else@test.com"));
        var otherHandler = new GetSurveyDetailQueryHandler(Fixture.Context, otherUserService, cache, _mockValidator);
        var otherResult = await otherHandler.Handle(new GetSurveyDetailQuery(1), CancellationToken.None);

        await Assert.That(ownerResult.IsSuccess).IsTrue();
        await Assert.That(otherResult.Error).IsEqualTo(Errors.General.Forbidden());
    }

    [Test]
    public async Task GivenSurveyIdWhichDoesNotExist_WhenCallingHandle_ThenNotFoundShouldNotBeCached()
    {
        const int id = 100;

        using var services = new ServiceCollection().AddHybridCache().Services.BuildServiceProvider();
        var cache = services.GetRequiredService<HybridCache>();
        var handler = new GetSurveyDetailQueryHandler(Fixture.Context, _mockUserService, cache, _mockValidator);

        var result = await handler.Handle(new GetSurveyDetailQuery(id), CancellationToken.None);

        // If the miss had been cached, the factory below would not run and the cached null would be returned.
        var cached = await cache.GetOrCreateAsync<string?>($"survey:{id}",
            _ => ValueTask.FromResult<string?>("created later"));

        await Assert.That(result.Error).IsEqualTo(Errors.General.NotFound());
        await Assert.That(cached).IsEqualTo("created later");
    }
}
