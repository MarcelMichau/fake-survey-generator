using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Surveys.Models;
using FakeSurveyGenerator.Application.Surveys.Queries.GetSurveyDetail;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using Shouldly;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Surveys.Queries.GetSurveyDetail
{
    [Collection("QueryTests")]
    public class GetSurveyDetailQueryTests
    {
        private readonly SurveyContext _surveyContext;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        public GetSurveyDetailQueryTests(QueryTestFixture fixture)
        {
            _surveyContext = fixture.Context;
            _mapper = fixture.Mapper;
            _cache = fixture.Cache;
        }

        [Fact]
        public async Task Handle_Returns_Correct_Type()
        {
            const int id = 1;

            var query = new GetSurveyDetailQuery(id);

            var handler = new GetSurveyDetailWithEntityFrameworkQueryHandler(_surveyContext, _mapper, _cache);

            var result = await handler.Handle(query, CancellationToken.None);

            result.ShouldBeOfType<Result<SurveyModel, Error>>();
        }

        [Fact]
        public async Task Handle_Returns_Correct_Id()
        {
            const int id = 1;

            var query = new GetSurveyDetailQuery(id);

            var handler = new GetSurveyDetailWithEntityFrameworkQueryHandler(_surveyContext, _mapper, _cache);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Value.Id.ShouldBe(id);
        }

        [Fact]
        public async Task Handle_Returns_Correct_Topic()
        {
            const int id = 1;
            const string expectedTopicText = "Test Topic 1";

            var query = new GetSurveyDetailQuery(id);

            var handler = new GetSurveyDetailWithEntityFrameworkQueryHandler(_surveyContext, _mapper, _cache);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Value.Topic.ShouldBe(expectedTopicText);
        }

        [Fact]
        public async Task Handle_Returns_Correct_Number_Of_Respondents()
        {
            const int id = 1;
            const int expectedNumberOfRespondents = 10;

            var query = new GetSurveyDetailQuery(id);

            var handler = new GetSurveyDetailWithEntityFrameworkQueryHandler(_surveyContext, _mapper, _cache);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Value.NumberOfRespondents.ShouldBe(expectedNumberOfRespondents);
        }

        [Fact]
        public async Task Handle_Returns_Correct_Respondent_Type()
        {
            const int id = 1;
            const string expectedTopicText = "Testers";

            var query = new GetSurveyDetailQuery(id);

            var handler = new GetSurveyDetailWithEntityFrameworkQueryHandler(_surveyContext, _mapper, _cache);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Value.RespondentType.ShouldBe(expectedTopicText);
        }

        [Fact]
        public async Task Handle_Returns_Error_When_Survey_Id_Does_Not_Exist()
        {
            const int id = 100;

            var query = new GetSurveyDetailQuery(id);

            var handler = new GetSurveyDetailWithEntityFrameworkQueryHandler(_surveyContext, _mapper, _cache);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Error.ShouldBe(Errors.General.NotFound());
        }
    }
}
