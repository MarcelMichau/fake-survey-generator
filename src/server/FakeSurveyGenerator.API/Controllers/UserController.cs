using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Users.Commands.RegisterUser;
using FakeSurveyGenerator.Application.Users.Queries.GetUser;
using FakeSurveyGenerator.Application.Users.Queries.IsUserRegistered;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FakeSurveyGenerator.API.Controllers
{
    [Authorize]
    public sealed class UserController : ApiController
    {
        /// <summary>
        /// Retrieves a specific User.
        /// </summary>
        /// <param name="id">Primary key of the User</param>
        /// <param name="cancellationToken">Automatically set by ASP.NET Core</param>
        /// <returns>The requested UserModel</returns>
        /// <response code="200">Returns the requested UserModel</response> 
        /// <response code="404">If the requested UserModel is not found</response> 
        [HttpGet("{id}", Name = nameof(GetUser))]
        public async Task<IActionResult> GetUser(int id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetUserQuery(id), cancellationToken);

            return FromResult(result);
        }

        /// <summary>
        /// Checks whether or not a User with a specific UserId is already registered in the system.
        /// </summary>
        /// <param name="userId">The external user identifier</param>
        /// <param name="cancellationToken">Automatically set by ASP.NET Core</param>
        /// <returns>Boolean result</returns>
        /// <response code="200">Boolean result indicating registered status</response> 
        [HttpGet("isRegistered")]
        public async Task<IActionResult> IsRegistered(string userId, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new IsUserRegisteredQuery(userId), cancellationToken);

            return FromResult(result);
        }

        /// <summary>
        /// Registers a new User, using the information from the access token.
        /// </summary>
        /// <returns>A newly registered UserModel</returns>
        /// <response code="201">Returns the newly registered User</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new RegisterUserCommand(), cancellationToken);

            return result.IsSuccess ? CreatedAtRoute(nameof(GetUser), new { id = result.Value.Id }, result.Value) : FromResult(result);
        }
    }
}
