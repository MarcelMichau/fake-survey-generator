using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace FakeSurveyGenerator.API.Tests.Integration.Controllers
{
    public sealed class AdminControllerTests : IClassFixture<IntegrationTestWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public AdminControllerTests(IntegrationTestWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_Should_Return_Ok()
        {
            var response = await _client.GetAsync("/api/admin/ping");
            response.EnsureSuccessStatusCode();
        }
    }
}
