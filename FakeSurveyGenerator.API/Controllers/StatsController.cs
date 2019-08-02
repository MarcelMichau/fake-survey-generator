using Microsoft.AspNetCore.Mvc;

namespace FakeSurveyGenerator.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        /// <summary>
        /// Returns a 200 OK Result. Used for testing network latency
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}