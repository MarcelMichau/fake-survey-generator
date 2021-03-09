using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AutoWrapper.Server;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using FakeSurveyGenerator.Application.Surveys.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    [Collection(nameof(IntegrationTestFixture))]
    public sealed class SurveyControllerTests
    {
        private readonly HttpClient _authenticatedClient;
        private readonly HttpClient _unauthenticatedClient;

        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public SurveyControllerTests(IntegrationTestFixture fixture)
        {
            _authenticatedClient = fixture.Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test", _ => { });
                });
            }).CreateDefaultClient(new UnwrappingResponseHandler());

            _unauthenticatedClient = fixture.Factory.CreateClient();
        }

        [Fact]
        public async Task GivenAuthenticatedClientWithValidCreateSurveyCommand_WhenCallingPostSurvey_ThenSuccessfulResponseWithNewlyCreatedSurveyShouldBeReturned()
        {
            var createSurveyCommand = new CreateSurveyCommand("How awesome is this?", 350, "Individuals",
                new List<SurveyOptionDto>
                {
                    new()
                    {
                        OptionText = "Very awesome"
                    },
                    new()
                    {
                        OptionText = "Not so much"
                    }
                });

            var response = await _authenticatedClient.PostAsJsonAsync("/api/survey", createSurveyCommand, Options);

            response.EnsureSuccessStatusCode();

            await using var content = await response.Content.ReadAsStreamAsync();

            var survey = await JsonSerializer.DeserializeAsync<SurveyModel>(content, Options);

            Assert.Equal(350, survey.Options.Sum(option => option.NumberOfVotes));
            Assert.Equal("How awesome is this?", survey.Topic);
            Assert.True(survey.Options.All(option => option.NumberOfVotes > 0));
            Assert.False(survey.CreatedOn == DateTimeOffset.MinValue);
            Assert.Equal("test-id", survey.CreatedBy);
        }

        [Fact]
        public async Task GivenUnauthenticatedClientWithValidCreateSurveyCommand_WhenCallingPostSurvey_ThenUnauthorizedResponseShouldBeReturned()
        {
            var createSurveyCommand = new CreateSurveyCommand("How unauthorized is this?", 400, "Unauthorized users",
                new List<SurveyOptionDto>
                {
                    new()
                    {
                        OptionText = "Very Unauthorized"
                    },
                    new()
                    {
                        OptionText = "Completely Unauthorized"
                    }
                });

            using var response = await _unauthenticatedClient.PostAsJsonAsync("/api/survey", createSurveyCommand);

            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GivenInvalidCreateSurveyCommand_WhenCallingPostSurvey_ThenUnprocessableEntityResponseShouldBeReturned()
        {
            var createSurveyCommand = new CreateSurveyCommand("", 0, "",
                new List<SurveyOptionDto>
                {
                    new()
                    {
                        OptionText = ""
                    }
                });

            using var response = await _authenticatedClient.PostAsJsonAsync("/api/survey", createSurveyCommand);

            var statusCode = (int) response.StatusCode;

            statusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        }

        [Fact]
        public async Task GivenExistingSurveyId_WhenCallingGetSurvey_ThenExistingSurveyShouldBeReturned()
        {
            const int surveyId = 1;

            const string expectedSurveyTopic = "Test Topic 1";
            const int expectedNumberOfRespondents = 10;
            const string expectedRespondentType = "Testers";

            const string expectedOptionText = "Test Option 1";

            var survey = await _authenticatedClient.GetFromJsonAsync<SurveyModel>($"api/survey/{surveyId}");

            survey.Id.Should().Be(surveyId);
            survey.Topic.Should().Be(expectedSurveyTopic);
            survey.NumberOfRespondents.Should().Be(expectedNumberOfRespondents);
            survey.RespondentType.Should().Be(expectedRespondentType);
            survey.Options.First().OptionText.Should().Be(expectedOptionText);
        }

        [Fact]
        public async Task GivenNonExistentSurveyId_WhenCallingGetSurvey_ThenNotFoundResponseShouldBeReturned()
        {
            const int surveyId = 100;

            var response = await _authenticatedClient.GetAsync($"api/survey/{surveyId}");

            var statusCode = (int) response.StatusCode;

            statusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}
