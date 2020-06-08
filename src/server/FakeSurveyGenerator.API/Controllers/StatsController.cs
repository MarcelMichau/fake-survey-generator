using Microsoft.AspNetCore.Mvc;

namespace FakeSurveyGenerator.API.Controllers
{
    public sealed class StatsController : ApiController
    {
        /// <summary>
        /// Returns a 200 OK Result. Used for testing network latency and as a sanity check
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}