using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Users.Commands.RegisterUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FakeSurveyGenerator.API.Controllers
{
    [Authorize]
    public class UserController : ApiController
    {
        [HttpGet("{id}", Name = nameof(GetUser))]
        public async Task<IActionResult> GetUser(int id, CancellationToken cancellationToken)
        {
            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);

            return result.IsSuccess ? CreatedAtRoute(nameof(GetUser), new { id = result.Value.Id }, result.Value) : FromResult(result);
        }
    }
}
