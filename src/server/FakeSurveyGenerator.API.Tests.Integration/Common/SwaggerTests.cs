using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration.Common
{
    [Collection(nameof(IntegrationTestFixture))]
    public sealed class SwaggerTests
    {
        private readonly HttpClient _client;

        public SwaggerTests(IntegrationTestFixture testFixture)
        {
            _client = testFixture.Factory.CreateClient();
        }

        [Fact]
        public async Task GivenAnyUser_WhenMakingRequestToSwaggerUiRoute_ThenSuccessResponseShouldBeReturned()
        {
            var response = await _client.GetAsync("/swagger");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GivenAnyUser_WhenMakingRequestToSwaggerJsonRoute_ThenSuccessResponseShouldBeReturned()
        {
            var response = await _client.GetAsync("/swagger/v1/swagger.json");
            response.EnsureSuccessStatusCode();
        }
    }
}
