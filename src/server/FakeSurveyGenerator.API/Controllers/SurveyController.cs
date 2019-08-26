using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.API.Application.Commands;
using FakeSurveyGenerator.API.Application.Models;
using FakeSurveyGenerator.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FakeSurveyGenerator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ISurveyQueries _surveyQueries;

        public SurveyController(IMediator mediator, ISurveyQueries surveyQueries)
        {
            _mediator = mediator;
            _surveyQueries = surveyQueries;
        }

        /// <summary>
        /// Retrieves a specific survey.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The requested SurveyModel</returns>
        /// <response code="200">Returns the requested survey</response> 
        /// <response code="404">If the requested survey is not found</response> 
        [HttpGet("{id}", Name = nameof(GetSurvey))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<SurveyModel>> GetSurvey(int id)
        {
            try
            {
                var result = await _surveyQueries.GetSurveyAsync(id);

                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Creates a new survey.
        /// </summary>
        /// <returns>A newly created SurveyModel</returns>
        /// <response code="201">Returns the newly created survey</response>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult<SurveyModel>> CreateSurvey([FromBody] CreateSurveyCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            return CreatedAtRoute(nameof(GetSurvey), new { id = result.Id }, result);
        }
    }
}