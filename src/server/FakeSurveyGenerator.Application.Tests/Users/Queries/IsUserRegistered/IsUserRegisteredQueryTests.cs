using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Users.Queries.IsUserRegistered;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Shouldly;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Users.Queries.IsUserRegistered
{
    [Collection("QueryTests")]
    public sealed class IsUserRegisteredQueryTests
    {
        private readonly SurveyContext _surveyContext;

        public IsUserRegisteredQueryTests(QueryTestFixture fixture)
        {
            _surveyContext = fixture.Context;
        }

        [Fact]
        public async Task Handle_Returns_Correct_Type()
        {
            const string userId = "test-id";

            var query = new IsUserRegisteredQuery(userId);

            var handler = new IsUserRegisteredQueryHandler(_surveyContext);

            var result = await handler.Handle(query, CancellationToken.None);

            result.ShouldBeOfType<Result<bool>>();
        }

        [Fact]
        public async Task Handle_Returns_True_For_Existing_User()
        {
            const string userId = "test-id";

            var query = new IsUserRegisteredQuery(userId);

            var handler = new IsUserRegisteredQueryHandler(_surveyContext);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Value.ShouldBe(true);
        }

        [Fact]
        public async Task Handle_Returns_False_For_Nonexistent_User()
        {
            const string userId = "unregistered-id";

            var query = new IsUserRegisteredQuery(userId);

            var handler = new IsUserRegisteredQueryHandler(_surveyContext);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Value.ShouldBe(false);
        }
    }
}
