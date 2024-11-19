using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Caching;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Tests.Setup;
using FluentAssertions;

namespace FakeSurveyGenerator.Application.Tests.Features.Surveys;

[Collection(nameof(QueryTestFixture))]
public sealed class GetSurveyDetailQueryTests(QueryTestFixture fixture)
{
    private readonly SurveyContext _surveyContext = fixture.Context;
    private readonly ICache<SurveyModel?> _cache = QueryTestFixture.GetCache<SurveyModel?>();

    [Fact]
    public async Task GivenExistingSurveyId_WhenCallingHandle_ThenExpectedResultTypeShouldBeReturned()
    {
        const int id = 1;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(_surveyContext, _cache);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeOfType<Result<SurveyModel, Error>>();
    }

    [Fact]
    public async Task GivenExistingSurveyId_WhenCallingHandle_ThenReturnedSurveyIdShouldMatchGivenSurveyId()
    {
        const int id = 1;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(_surveyContext, _cache);

        var result = await handler.Handle(query, CancellationToken.None);

        var survey = result.Value;

        survey.Id.Should().Be(id);
    }

    [Fact]
    public async Task GivenExistingSurveyId_WhenCallingHandle_ThenReturnedSurveyTopicShouldMatchExpectedValue()
    {
        const int id = 1;
        const string expectedTopicText = "Test Topic 1";

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(_surveyContext, _cache);

        var result = await handler.Handle(query, CancellationToken.None);

        var survey = result.Value;

        survey.Topic.Should().Be(expectedTopicText);
    }

    [Fact]
    public async Task
        GivenExistingSurveyId_WhenCallingHandle_ThenReturnedSurveyNumberOfRespondentsShouldMatchExpectedValue()
    {
        const int id = 1;
        const int expectedNumberOfRespondents = 10;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(_surveyContext, _cache);

        var result = await handler.Handle(query, CancellationToken.None);

        var survey = result.Value;

        survey.NumberOfRespondents.Should().Be(expectedNumberOfRespondents);
    }

    [Fact]
    public async Task GivenExistingSurveyId_WhenCallingHandle_ThenReturnedSurveyRespondentTypeShouldMatchExpectedValue()
    {
        const int id = 1;
        const string expectedTopicText = "Testers";

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(_surveyContext, _cache);

        var result = await handler.Handle(query, CancellationToken.None);

        var survey = result.Value;

        survey.RespondentType.Should().Be(expectedTopicText);
    }

    [Fact]
    public async Task GivenSurveyIdWhichDoesNotExist_WhenCallingHandle_ThenResponseShouldIndicateNotFoundError()
    {
        const int id = 100;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(_surveyContext, _cache);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Error.Should().Be(Errors.General.NotFound());
    }
}