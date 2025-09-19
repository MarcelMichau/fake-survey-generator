using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Shared.Caching;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Tests.Setup;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace FakeSurveyGenerator.Application.Tests.Features.Surveys;

public sealed class GetSurveyDetailQueryTests
{
    [ClassDataSource<TestFixture>]
    public required TestFixture Fixture { get; init; }
    private static ICache<SurveyModel?> Cache => TestFixture.GetCache<SurveyModel?>();
    private readonly IValidator<GetSurveyDetailQuery> _mockValidator = Substitute.For<IValidator<GetSurveyDetailQuery>>();

    public GetSurveyDetailQueryTests()
    {
        // Setup mock validator to always return successful validation
        _mockValidator.ValidateAsync(Arg.Any<GetSurveyDetailQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    [Test]
    public async Task GivenExistingSurveyId_WhenCallingHandle_ThenExpectedResultTypeShouldBeReturned()
    {
        const int id = 1;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(Fixture.Context, Cache, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That((object)result).IsTypeOf<Result<SurveyModel, Error>>();
    }

    [Test]
    public async Task GivenExistingSurveyId_WhenCallingHandle_ThenReturnedSurveyIdShouldMatchGivenSurveyId()
    {
        const int id = 1;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(Fixture.Context, Cache, _mockValidator);

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

        var handler = new GetSurveyDetailQueryHandler(Fixture.Context, Cache, _mockValidator);

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

        var handler = new GetSurveyDetailQueryHandler(Fixture.Context, Cache, _mockValidator);

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

        var handler = new GetSurveyDetailQueryHandler(Fixture.Context, Cache, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        var survey = result.Value;

        await Assert.That(survey.RespondentType).IsEqualTo(expectedTopicText);
    }

    [Test]
    public async Task GivenSurveyIdWhichDoesNotExist_WhenCallingHandle_ThenResponseShouldIndicateNotFoundError()
    {
        const int id = 100;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(Fixture.Context, Cache, _mockValidator);

        var result = await handler.Handle(query, CancellationToken.None);

        await Assert.That(result.Error).IsEqualTo(Errors.General.NotFound());
    }
}