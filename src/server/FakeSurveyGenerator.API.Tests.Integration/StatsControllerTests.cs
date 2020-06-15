using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration
{
    public sealed class StatsControllerTests : IClassFixture<IntegrationTestWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public StatsControllerTests(IntegrationTestWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_Should_Return_Ok()
        {
            var response = await _client.GetAsync("/api/stats/ping");
            response.EnsureSuccessStatusCode();
        }
    }
}
