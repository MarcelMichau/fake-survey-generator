using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.API.Application.Commands;
using FakeSurveyGenerator.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FakeSurveyGenerator.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSurvey(int id)
        {
            var result = await _surveyQueries.GetSurveyAsync(id);

            return Ok(result);
        }

        /// <summary>
        /// Creates a new survey.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSurvey([FromBody] CreateSurveyCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }
    }
}