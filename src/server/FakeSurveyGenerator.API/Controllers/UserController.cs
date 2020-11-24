using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Models;
using FakeSurveyGenerator.Application.Users.Commands.RegisterUser;
using FakeSurveyGenerator.Application.Users.Models;
using FakeSurveyGenerator.Application.Users.Queries.GetUser;
using FakeSurveyGenerator.Application.Users.Queries.IsUserRegistered;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FakeSurveyGenerator.API.Controllers
{
    [Authorize]
    [SwaggerTag("Create & read Users")]
    public sealed class UserController : ApiController
    {
        [HttpGet("{id}", Name = nameof(GetUser))]
        [SwaggerOperation("Retrieves a specific User")]
        [SwaggerResponse(StatusCodes.Status200OK, "The requested User was found", typeof(ApiResultResponse<UserModel>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The requested User was not found")]
        public async Task<IActionResult> GetUser(int id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetUserQuery(id), cancellationToken);

            return FromResult(result);
        }

        [HttpGet("isRegistered")]
        [SwaggerOperation("Checks whether or not a User with a specific UserId is already registered in the system")]
        [SwaggerResponse(StatusCodes.Status200OK, "User registration status", typeof(ApiResultResponse<UserRegistrationStatusModel>))]
        public async Task<IActionResult> IsRegistered([SwaggerParameter("The external user identifier", Required = true)] string userId, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new IsUserRegisteredQuery(userId), cancellationToken);

            return FromResult(result);
        }

        [HttpPost("register")]
        [SwaggerOperation("Registers a new User, using the information from the access token")]
        [SwaggerResponse(StatusCodes.Status201Created, "The new User was registered", typeof(ApiResultResponse<UserModel>))]
        public async Task<IActionResult> Register(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new RegisterUserCommand(), cancellationToken);

            return result.IsSuccess ? CreatedAtRoute(nameof(GetUser), new { id = result.Value.Id }, result.Value) : FromResult(result);
        }
    }
}
