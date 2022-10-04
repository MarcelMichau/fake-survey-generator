using AutoMapper;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Users.Models;
using FakeSurveyGenerator.Application.Users.Queries.GetUser;
using FakeSurveyGenerator.Infrastructure.Persistence;
using FluentAssertions;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Users.Queries.GetUser;

[Collection(nameof(QueryTestFixture))]
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
    public async Task GivenExistingUserId_WhenCallingHandle_ThenExpectedResultTypeShouldBeReturned()
    {
        const int id = 1;

        var query = new GetUserQuery(id);

        var handler = new GetUserQueryHandler(_surveyContext, _mapper);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeOfType<Result<UserModel, Error>>();
    }

    [Fact]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenReturnedUserIdShouldMatchGivenUserId()
    {
        const int id = 1;

        var query = new GetUserQuery(id);

        var handler = new GetUserQueryHandler(_surveyContext, _mapper);

        var result = await handler.Handle(query, CancellationToken.None);

        var user = result.Value;

        user.Id.Should().Be(id);
    }

    [Fact]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenReturnedDisplayNameShouldMatchExpectedValue()
    {
        const int id = 1;
        const string expectedDisplayName = "Test User";

        var query = new GetUserQuery(id);

        var handler = new GetUserQueryHandler(_surveyContext, _mapper);

        var result = await handler.Handle(query, CancellationToken.None);

        var user = result.Value;

        user.DisplayName.Should().Be(expectedDisplayName);
    }

    [Fact]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenReturnedEmailAddressShouldMatchExpectedValue()
    {
        const int id = 1;
        const string expectedEmailAddress = "test.user@test.com";

        var query = new GetUserQuery(id);

        var handler = new GetUserQueryHandler(_surveyContext, _mapper);

        var result = await handler.Handle(query, CancellationToken.None);

        var user = result.Value;

        user.EmailAddress.Should().Be(expectedEmailAddress);
    }

    [Fact]
    public async Task GivenExistingUserId_WhenCallingHandle_ThenReturnedExternalUserIdShouldMatchExpectedValue()
    {
        const int id = 1;
        const string expectedExternalUserId = "test-id";

        var query = new GetUserQuery(id);

        var handler = new GetUserQueryHandler(_surveyContext, _mapper);

        var result = await handler.Handle(query, CancellationToken.None);

        var user = result.Value;

        user.ExternalUserId.Should().Be(expectedExternalUserId);
    }
}