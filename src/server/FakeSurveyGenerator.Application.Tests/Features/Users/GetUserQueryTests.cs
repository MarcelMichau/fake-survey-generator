using AutoMapper;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Tests.Setup;
using FluentAssertions;

namespace FakeSurveyGenerator.Application.Tests.Features.Users;

[Collection(nameof(QueryTestFixture))]
public sealed class GetUserQueryTests(QueryTestFixture fixture)
{
    private readonly SurveyContext _surveyContext = fixture.Context;
    private readonly IMapper _mapper = fixture.Mapper;

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