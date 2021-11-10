using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Models.ResponseTypes;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using FakeSurveyGenerator.Application.Surveys.Models;
using FakeSurveyGenerator.Application.Surveys.Queries.GetSurveyDetail;
using FakeSurveyGenerator.Application.Surveys.Queries.GetUserSurveys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FakeSurveyGenerator.API.Controllers;

[Authorize]
[SwaggerTag("Create & read Surveys")]
public sealed class SurveyController : ApiController
{
    [HttpGet("{id:int}", Name = nameof(GetSurvey))]
    [SwaggerOperation("Retrieves a specific Survey")]
    [SwaggerResponse(StatusCodes.Status200OK, "The requested survey was found", typeof(ApiResultResponse<SurveyModel>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The requested Survey was not found")]
    public async Task<IActionResult> GetSurvey([SwaggerParameter("Primary key of the Survey")] int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetSurveyDetailQuery(id), cancellationToken);

        return FromResult(result);
    }

    [HttpGet("user")]
    [SwaggerOperation("Retrieves all Surveys created by the current user")]
    [SwaggerResponse(StatusCodes.Status200OK, "Surveys were found for the current user", typeof(ApiResultResponse<List<UserSurveyModel>>))]
    public async Task<IActionResult> GetUserSurveys(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetUserSurveysQuery(), cancellationToken);

        return FromResult(result);
    }

    [HttpPost]
    [SwaggerOperation("Creates a new Survey")]
    [SwaggerResponse(StatusCodes.Status201Created, "The Survey was created", typeof(ApiResultResponse<SurveyModel>))]
    [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "The Survey creation command contains validation errors")]
    public async Task<IActionResult> CreateSurvey([SwaggerParameter("Command to create new Survey", Required = true)] CreateSurveyCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);

        return result.IsSuccess ? CreatedAtRoute(nameof(GetSurvey), new { id = result.Value.Id }, result.Value) : FromResult(result);
    }
}