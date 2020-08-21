using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Exceptions;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using FakeSurveyGenerator.Application.Surveys.Queries.GetSurveyDetail;
using FakeSurveyGenerator.Application.Surveys.Queries.GetUserSurveys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FakeSurveyGenerator.API.Controllers
{
    [Authorize]
    public sealed class SurveyController : ApiController
    {
        /// <summary>
        /// Retrieves a specific Survey.
        /// </summary>
        /// <param name="id">Primary key of the Survey</param>
        /// <param name="cancellationToken">Automatically set by ASP.NET Core</param>
        /// <returns>The requested SurveyModel</returns>
        /// <response code="200">Returns the requested SurveyModel</response> 
        /// <response code="404">If the requested Survey is not found</response> 
        [HttpGet("{id}", Name = nameof(GetSurvey))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetSurvey(int id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetSurveyDetailQuery(id), cancellationToken);

            return FromResult(result);
        }

        /// <summary>
        /// Retrieves all Surveys created by the current user.
        /// </summary>
        /// <param name="cancellationToken">Automatically set by ASP.NET Core</param>
        /// <returns>The current user's SurveyModels</returns>
        /// <response code="200">Returns all SurveyModels for the current user</response> 
        [HttpGet("user")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserSurveys(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetUserSurveysQuery(), cancellationToken);

            return FromResult(result);
        }

        /// <summary>
        /// Creates a new Survey.
        /// </summary>
        /// <returns>A newly created SurveyModel</returns>
        /// <response code="201">Returns the newly created SurveyModel</response>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateSurvey(CreateSurveyCommand command, CancellationToken cancellationToken)
        {
            try // Temporary try/catch until this issue is fixed: https://github.com/FluentValidation/FluentValidation/issues/1431
            {
                var result = await Mediator.Send(command, cancellationToken);

                return result.IsSuccess ? CreatedAtRoute(nameof(GetSurvey), new { id = result.Value.Id }, result.Value) : FromResult(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Failures);
            }
        }
    }
}