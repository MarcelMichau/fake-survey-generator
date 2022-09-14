using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FakeSurveyGenerator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    private ISender _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetService<ISender>();

    protected IActionResult FromResult<T>(Result<T> result)
    {
        return Ok(result.Value);
    }

    protected IActionResult FromResult<T>(Result<T, Error> result)
    {
        return result.IsSuccess
            ? Ok(result.Value)
            : BuildProblemDetails(result,
                Equals(result.Error, Errors.General.NotFound())
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status400BadRequest);
    }

    private IActionResult BuildProblemDetails<T>(Result<T, Error> result, int statusCode)
    {
        return Problem($"Error Code: {result.Error.Code}. Error Message: {result.Error.Message}",
            statusCode: statusCode);
    }
}