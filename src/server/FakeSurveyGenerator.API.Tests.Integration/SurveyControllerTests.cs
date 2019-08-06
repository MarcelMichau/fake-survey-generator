using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using FakeSurveyGenerator.API.Application.Commands;
using FakeSurveyGenerator.API.Application.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    public class SurveyControllerTests : IClassFixture<InMemoryDatabaseWebApplicationFactory<Startup>>
    {
        private readonly InMemoryDatabaseWebApplicationFactory<Startup> _factory;

        public SurveyControllerTests(InMemoryDatabaseWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Post_Test()
        {
            var client = _factory.CreateClient();

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

            var response = await client.PostAsync("/api/survey", new StringContent(JsonConvert.SerializeObject(createSurveyCommand), Encoding.UTF8, MediaTypeNames.Application.Json));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var surveyResult = JsonConvert.DeserializeObject<SurveyModel>(content);

            Assert.Equal(350, surveyResult.Options.Sum(option => option.NumberOfVotes));
            Assert.Equal("How awesome is this?", surveyResult.Topic);
            Assert.True(surveyResult.Options.All(option => option.NumberOfVotes > 0));
        }
    }
}
