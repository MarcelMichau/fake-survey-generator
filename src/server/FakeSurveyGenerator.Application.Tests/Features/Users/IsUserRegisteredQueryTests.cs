using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Tests.Setup;
using FluentAssertions;

namespace FakeSurveyGenerator.Application.Tests.Features.Users;

[Collection(nameof(QueryTestFixture))]
public sealed class IsUserRegisteredQueryTests(QueryTestFixture fixture)
{
    private readonly SurveyContext _surveyContext = fixture.Context;

    [Fact]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenExpectedResultTypeShouldBeReturned()
    {
        const string userId = "test-id";

        var query = new IsUserRegisteredQuery(userId);

        var handler = new IsUserRegisteredQueryHandler(_surveyContext);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeOfType<Result<UserRegistrationStatusModel>>();
    }

    [Fact]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenReturnedResultShouldBeTrue()
    {
        const string userId = "test-id";

        var query = new IsUserRegisteredQuery(userId);

        var handler = new IsUserRegisteredQueryHandler(_surveyContext);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.IsUserRegistered.Should().BeTrue();
    }

    [Fact]
    public async Task GivenNewUserId_WhenCallingHandle_ThenReturnedResultShouldBeFalse()
    {
        const string userId = "unregistered-id";

        var query = new IsUserRegisteredQuery(userId);

        var handler = new IsUserRegisteredQueryHandler(_surveyContext);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Value.IsUserRegistered.Should().BeFalse();
    }
}