using System.Linq;
using System.Threading.Tasks;
using FakeSurveyGenerator.API.Models;
using Newtonsoft.Json;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    public class BasicTest : IClassFixture<InMemoryDatabaseWebApplicationFactory>
    {
        private readonly InMemoryDatabaseWebApplicationFactory _factory;

        public BasicTest(InMemoryDatabaseWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Post_Test()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsync("/api/survey", null);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var surveyResult = JsonConvert.DeserializeObject<SurveyModel>(content);

            Assert.Equal(1500, surveyResult.Options.Sum(option => option.NumberOfVotes));
            Assert.True(surveyResult.Options.All(option => option.NumberOfVotes > 0));
        }
    }
}
