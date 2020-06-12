using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiController : ControllerBase
    {
        private IMediator _mediator;

        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

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
}