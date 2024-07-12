using FakeSurveyGenerator.Api.Shared;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FakeSurveyGenerator.Api.Surveys;

internal static class SurveyEndpoints
{
    internal static void MapSurveyEndpoints(this WebApplication app)
    {
        var surveyGroup = app.MapGroup("/api/survey")
            .RequireAuthorization();

        surveyGroup.MapGet("/{id:int}", GetSurvey)
            .WithName(nameof(GetSurvey))
            .WithSummary("Retrieves a specific Survey")
            .WithOpenApi(generatedOperation =>
            {
                var parameter = generatedOperation.Parameters[0];
                parameter.Description = "Primary key of the Survey";
                return generatedOperation;
            });

        surveyGroup.MapGet("/user", GetUserSurveys)
            .WithName(nameof(GetUserSurveys))
            .WithSummary("Retrieves all Surveys created by the current user")
            .WithOpenApi();

        surveyGroup.MapPost("", CreateSurvey)
            .WithName(nameof(CreateSurvey))
            .WithSummary("Creates a new Survey")
            .WithOpenApi();
    }

    private static async Task<Results<Ok<SurveyModel>, ProblemHttpResult>> GetSurvey(ISender mediator, int id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSurveyDetailQuery(id), cancellationToken);

        return ResultExtensions.FromResult(result);
    }

    private static async Task<Results<Ok<List<UserSurveyModel>>, ProblemHttpResult>> GetUserSurveys(ISender mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserSurveysQuery(), cancellationToken);

        return ResultExtensions.FromResult(result);
    }

    private static async
        Task<Results<CreatedAtRoute<SurveyModel>, ProblemHttpResult,
            UnprocessableEntity<IDictionary<string, string[]>>>> CreateSurvey(ISender mediator,
            CreateSurveyCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return TypedResults.CreatedAtRoute(result.Value, nameof(GetSurvey), new { id = result.Value.Id });

            return TypedResults.Problem($"Error Code: {result.Error.Code}. Error Message: {result.Error.Message}",
                statusCode: StatusCodes.Status400BadRequest);
        }
        catch (ValidationException e)
        {
            return TypedResults.UnprocessableEntity(e.Errors);
        }
    }
}