using AutoMapper;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Caching;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Surveys.Models;
using FakeSurveyGenerator.Application.Surveys.Queries.GetSurveyDetail;
using FakeSurveyGenerator.Infrastructure.Persistence;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Surveys.Queries.GetSurveyDetail;

[Collection(nameof(QueryTestFixture))]
public sealed class GetSurveyDetailQueryTests
{
    private readonly SurveyContext _surveyContext;
    private readonly IMapper _mapper;
    private readonly ICache<SurveyModel> _cache;

    public GetSurveyDetailQueryTests(QueryTestFixture fixture)
    {
        _surveyContext = fixture.Context;
        _mapper = fixture.Mapper;
        _cache = Substitute.For<ICache<SurveyModel>>();
    }

    [Fact]
    public async Task GivenExistingSurveyId_WhenCallingHandle_ThenExpectedResultTypeShouldBeReturned()
    {
        const int id = 1;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(_surveyContext, _mapper, _cache);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeOfType<Result<SurveyModel, Error>>();
    }

    [Fact]
    public async Task GivenExistingSurveyId_WhenCallingHandle_ThenReturnedSurveyIdShouldMatchGivenSurveyId()
    {
        const int id = 1;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(_surveyContext, _mapper, _cache);

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

        var handler = new GetSurveyDetailQueryHandler(_surveyContext, _mapper, _cache);

        var result = await handler.Handle(query, CancellationToken.None);

        var survey = result.Value;

        survey.Topic.Should().Be(expectedTopicText);
    }

    [Fact]
    public async Task GivenExistingSurveyId_WhenCallingHandle_ThenReturnedSurveyNumberOfRespondentsShouldMatchExpectedValue()
    {
        const int id = 1;
        const int expectedNumberOfRespondents = 10;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(_surveyContext, _mapper, _cache);

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

        var handler = new GetSurveyDetailQueryHandler(_surveyContext, _mapper, _cache);

        var result = await handler.Handle(query, CancellationToken.None);

        var survey = result.Value;

        survey.RespondentType.Should().Be(expectedTopicText);
    }

    [Fact]
    public async Task GivenSurveyIdWhichDoesNotExist_WhenCallingHandle_ThenResponseShouldIndicateNotFoundError()
    {
        const int id = 100;

        var query = new GetSurveyDetailQuery(id);

        var handler = new GetSurveyDetailQueryHandler(_surveyContext, _mapper, _cache);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Error.Should().Be(Errors.General.NotFound());
    }
}