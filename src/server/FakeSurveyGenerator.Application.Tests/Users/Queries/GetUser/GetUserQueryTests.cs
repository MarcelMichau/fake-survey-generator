using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Users.Models;
using FakeSurveyGenerator.Application.Users.Queries.GetUser;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Shouldly;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Users.Queries.GetUser
{
    [Collection("QueryTests")]
    public sealed class GetUserQueryTests
    {
        private readonly SurveyContext _surveyContext;
        private readonly IMapper _mapper;

        public GetUserQueryTests(QueryTestFixture fixture)
        {
            _surveyContext = fixture.Context;
            _mapper = fixture.Mapper;
        }

        [Fact]
        public async Task Handle_Returns_Correct_Type()
        {
            const int id = 1;

            var query = new GetUserQuery(id);

            var handler = new GetUserQueryHandler(_surveyContext, _mapper);

            var result = await handler.Handle(query, CancellationToken.None);

            result.ShouldBeOfType<Result<UserModel, Error>>();
        }

        [Fact]
        public async Task Handle_Returns_Correct_Id()
        {
            const int id = 1;

            var query = new GetUserQuery(id);

            var handler = new GetUserQueryHandler(_surveyContext, _mapper);

            var result = await handler.Handle(query, CancellationToken.None);

            var user = result.Value;

            user.Id.ShouldBe(id);
        }

        [Fact]
        public async Task Handle_Returns_Correct_DisplayName()
        {
            const int id = 1;
            const string expectedDisplayName = "Test User";

            var query = new GetUserQuery(id);

            var handler = new GetUserQueryHandler(_surveyContext, _mapper);

            var result = await handler.Handle(query, CancellationToken.None);

            var user = result.Value;

            user.DisplayName.ShouldBe(expectedDisplayName);
        }

        [Fact]
        public async Task Handle_Returns_Correct_EmailAddress()
        {
            const int id = 1;
            const string expectedEmailAddress = "test.user@test.com";

            var query = new GetUserQuery(id);

            var handler = new GetUserQueryHandler(_surveyContext, _mapper);

            var result = await handler.Handle(query, CancellationToken.None);

            var user = result.Value;

            user.EmailAddress.ShouldBe(expectedEmailAddress);
        }

        [Fact]
        public async Task Handle_Returns_Correct_ExternalUserId()
        {
            const int id = 1;
            const string expectedExternalUserId = "test-id";

            var query = new GetUserQuery(id);

            var handler = new GetUserQueryHandler(_surveyContext, _mapper);

            var result = await handler.Handle(query, CancellationToken.None);

            var user = result.Value;

            user.ExternalUserId.ShouldBe(expectedExternalUserId);
        }
    }
}
