using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using FakeSurveyGenerator.Application.Surveys.Models;
using FakeSurveyGenerator.Application.Users.Commands.RegisterUser;
using FakeSurveyGenerator.Application.Users.Models;
using Microsoft.AspNetCore.Http;
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

        public SurveyControllerTests(IntegrationTestFixture testFixture)
        {
            var fixture = new Fixture();

            _authenticatedClient = testFixture.Factory
                .WithSpecificUser(fixture.Create<string>(), fixture.Create<string>(), fixture.Create<string>());

            _unauthenticatedClient = testFixture.Factory.CreateClient();
        }

        [Fact]
        public async Task GivenAuthenticatedClientWithValidCreateSurveyCommand_WhenCallingPostSurvey_ThenSuccessfulResponseWithNewlyCreatedSurveyShouldBeReturned()
        {
            var newUser = await RegisterNewUser();
            var newSurvey = await CreateSurvey();

            Assert.Equal(350, newSurvey.Options.Sum(option => option.NumberOfVotes));
            Assert.Equal("How awesome is this?", newSurvey.Topic);
            Assert.True(newSurvey.Options.All(option => option.NumberOfVotes > 0));
            Assert.False(newSurvey.CreatedOn == DateTimeOffset.MinValue);
            Assert.Equal(newUser.ExternalUserId, newSurvey.CreatedBy);
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
            await RegisterNewUser();
            var newSurvey = await CreateSurvey();

            var survey = await _authenticatedClient.GetFromJsonAsync<SurveyModel>($"api/survey/{newSurvey.Id}");

            survey.Id.Should().Be(newSurvey.Id);
            survey.Topic.Should().Be(newSurvey.Topic);
            survey.NumberOfRespondents.Should().Be(newSurvey.NumberOfRespondents);
            survey.RespondentType.Should().Be(newSurvey.RespondentType);
            survey.Options.First().OptionText.Should().Be(newSurvey.Options.First().OptionText);
        }

        [Fact]
        public async Task GivenNonExistentSurveyId_WhenCallingGetSurvey_ThenNotFoundResponseShouldBeReturned()
        {
            const int surveyId = 600000;

            var response = await _authenticatedClient.GetAsync($"api/survey/{surveyId}");

            var statusCode = (int) response.StatusCode;

            statusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        private async Task<UserModel> RegisterNewUser()
        {
            var registerUserCommand = new RegisterUserCommand();

            var response = await _authenticatedClient.PostAsJsonAsync("/api/user/register", registerUserCommand, Options);

            response.EnsureSuccessStatusCode();

            await using var content = await response.Content.ReadAsStreamAsync();

            var user = await JsonSerializer.DeserializeAsync<UserModel>(content, Options);
            return user;
        }

        private async Task<SurveyModel> CreateSurvey()
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
            return survey;
        }
    }
}
