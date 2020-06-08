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
            if (result.IsSuccess)
                return Ok(result.Value);

            if (Equals(result.Error, Errors.General.NotFound()))
                return Problem($"Error Code: {result.Error.Code}. Error Message: {result.Error.Message}", statusCode: StatusCodes.Status404NotFound);

            return Problem($"Error Code: {result.Error.Code}. Error Message: {result.Error.Message}", statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
