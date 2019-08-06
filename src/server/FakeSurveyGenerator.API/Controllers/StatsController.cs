using Microsoft.AspNetCore.Mvc;

namespace FakeSurveyGenerator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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