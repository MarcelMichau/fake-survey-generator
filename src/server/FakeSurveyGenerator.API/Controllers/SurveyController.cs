using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using FakeSurveyGenerator.Application.Surveys.Queries.GetSurveyDetail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FakeSurveyGenerator.API.Controllers
{
    [Authorize]
    public sealed class SurveyController : ApiController
    {
        /// <summary>
        /// Retrieves a specific survey.
        /// </summary>
        /// <param name="id">Primary key of the Survey</param>
        /// <param name="cancellationToken">Automatically set by ASP.NET Core</param>
        /// <returns>The requested SurveyModel</returns>
        /// <response code="200">Returns the requested survey</response> 
        /// <response code="404">If the requested survey is not found</response> 
        [HttpGet("{id}", Name = nameof(GetSurvey))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetSurvey(int id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetSurveyDetailQuery(id), cancellationToken);

            return FromResult(result);
        }

        /// <summary>
        /// Creates a new survey.
        /// </summary>
        /// <returns>A newly created SurveyModel</returns>
        /// <response code="201">Returns the newly created survey</response>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateSurvey([FromBody] CreateSurveyCommand command, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);

            return result.IsSuccess ? CreatedAtRoute(nameof(GetSurvey), new { id = result.Value.Id }, result.Value) : FromResult(result);
        }
    }
}