using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FakeSurveyGenerator.Application.Surveys.Models;
using FakeSurveyGenerator.Application.Surveys.Queries.GetSurveyDetail;
using FakeSurveyGenerator.Infrastructure;
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

            result.ShouldBeOfType<SurveyModel>();
        }

        [Fact]
        public async Task Handle_Returns_Correct_Id()
        {
            const int id = 1;

            var query = new GetSurveyDetailQuery(id);

            var handler = new GetSurveyDetailWithEntityFrameworkQueryHandler(_surveyContext, _mapper, _cache);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Id.ShouldBe(id);
        }

        [Fact]
        public async Task Handle_Returns_Correct_Topic()
        {
            const int id = 1;
            const string expectedTopicText = "Test Topic 1";

            var query = new GetSurveyDetailQuery(id);

            var handler = new GetSurveyDetailWithEntityFrameworkQueryHandler(_surveyContext, _mapper, _cache);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Topic.ShouldBe(expectedTopicText);
        }

        [Fact]
        public async Task Handle_Returns_Correct_Number_Of_Respondents()
        {
            const int id = 1;
            const int expectedNumberOfRespondents = 10;

            var query = new GetSurveyDetailQuery(id);

            var handler = new GetSurveyDetailWithEntityFrameworkQueryHandler(_surveyContext, _mapper, _cache);

            var result = await handler.Handle(query, CancellationToken.None);

            result.NumberOfRespondents.ShouldBe(expectedNumberOfRespondents);
        }

        [Fact]
        public async Task Handle_Returns_Correct_Respondent_Type()
        {
            const int id = 1;
            const string expectedTopicText = "Testers";

            var query = new GetSurveyDetailQuery(id);

            var handler = new GetSurveyDetailWithEntityFrameworkQueryHandler(_surveyContext, _mapper, _cache);

            var result = await handler.Handle(query, CancellationToken.None);

            result.RespondentType.ShouldBe(expectedTopicText);
        }

        [Fact]
        public async Task Handle_Throws_Exception_When_Survey_Id_Does_Not_Exist()
        {
            const int id = 100;

            var query = new GetSurveyDetailQuery(id);

            var handler = new GetSurveyDetailWithEntityFrameworkQueryHandler(_surveyContext, _mapper, _cache);

            await Should.ThrowAsync<KeyNotFoundException>(async () =>
            {
                await handler.Handle(query, CancellationToken.None);
            });
        }
    }
}
