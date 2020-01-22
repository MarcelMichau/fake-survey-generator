using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using FakeSurveyGenerator.Application.Surveys.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    public class SurveyControllerTests : IClassFixture<IntegrationTestWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _authenticatedClient;
        private readonly HttpClient _unauthenticatedClient;

        public SurveyControllerTests(IntegrationTestWebApplicationFactory<Startup> factory)
        {
            _authenticatedClient = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test", options => { });
                });
            }).CreateClient();

            _unauthenticatedClient = factory.CreateClient();
        }

        [Fact]
        public async Task Authenticated_Call_To_Post_Survey_Should_Create_Survey()
        {
            var createSurveyCommand = new CreateSurveyCommand("How awesome is this?", 350, "Individuals",
                new List<SurveyOptionDto>
                {
                    new SurveyOptionDto
                    {
                        OptionText = "Very awesome"
                    },
                    new SurveyOptionDto
                    {
                        OptionText = "Not so much"
                    }
                });

            var response = await _authenticatedClient.PostAsync("/api/survey", new StringContent(JsonConvert.SerializeObject(createSurveyCommand), Encoding.UTF8, MediaTypeNames.Application.Json));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var surveyResult = JsonConvert.DeserializeObject<SurveyModel>(content);

            Assert.Equal(350, surveyResult.Options.Sum(option => option.NumberOfVotes));
            Assert.Equal("How awesome is this?", surveyResult.Topic);
            Assert.True(surveyResult.Options.All(option => option.NumberOfVotes > 0));
        }

        [Fact]
        public async Task Unauthenticated_Call_To_Post_Survey_Should_Return_Unauthorized_Response()
        {
            var createSurveyCommand = new CreateSurveyCommand("How unauthorized is this?", 400, "Unauthorized users",
                new List<SurveyOptionDto>
                {
                    new SurveyOptionDto
                    {
                        OptionText = "Very unauthorized"
                    },
                    new SurveyOptionDto
                    {
                        OptionText = "Completely Unauthorized"
                    }
                });

            var response = await _unauthenticatedClient.PostAsync("/api/survey", new StringContent(JsonConvert.SerializeObject(createSurveyCommand), Encoding.UTF8, MediaTypeNames.Application.Json));

            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task Given_Invalid_CreateSurveyCommand_Post_Survey_Should_Return_Bad_Request()
        {
            var createSurveyCommand = new CreateSurveyCommand("", 0, "",
                new List<SurveyOptionDto>
                {
                    new SurveyOptionDto
                    {
                        OptionText = ""
                    }
                });

            var response = await _authenticatedClient.PostAsync("/api/survey", new StringContent(JsonConvert.SerializeObject(createSurveyCommand), Encoding.UTF8, MediaTypeNames.Application.Json));

            var statusCode = (int) response.StatusCode;

            statusCode.ShouldBe(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public async Task Get_Survey_Should_Return_Survey()
        {
            const int surveyId = 1;

            const string expectedSurveyTopic = "Test Topic 1";
            const int expectedNumberOfRespondents = 10;
            const string expectedRespondentType = "Testers";

            const string expectedOptionText = "Test Option 1";

            var response = await _authenticatedClient.GetStringAsync($"api/survey/{surveyId}");

            var surveyResult = JsonConvert.DeserializeObject<SurveyModel>(response);

            surveyResult.Id.ShouldBe(surveyId);
            surveyResult.Topic.ShouldBe(expectedSurveyTopic);
            surveyResult.NumberOfRespondents.ShouldBe(expectedNumberOfRespondents);
            surveyResult.RespondentType.ShouldBe(expectedRespondentType);
            surveyResult.Options.First().OptionText.ShouldBe(expectedOptionText);
        }
    }
}
