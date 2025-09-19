using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Api.Shared;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Features.Surveys;
using FakeSurveyGenerator.Application.Shared.Errors;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel;
using FakeSurveyGenerator.Api.Filters;

namespace FakeSurveyGenerator.Api.Surveys;

internal static class SurveyEndpoints
{
    internal static void MapSurveyEndpoints(this IEndpointRouteBuilder app)
    {
        var surveyGroup = app.MapGroup("/api/survey")
            .RequireAuthorization()
            .AddEndpointFilter<RequestLoggingEndpointFilter>();

        surveyGroup.MapGet("/{id:int}", GetSurvey)
            .WithName(nameof(GetSurvey))
            .WithSummary("Retrieves a specific Survey");

        surveyGroup.MapGet("/user", GetUserSurveys)
            .WithName(nameof(GetUserSurveys))
            .WithSummary("Retrieves all Surveys created by the current user");

        surveyGroup.MapPost("", CreateSurvey)
            .WithName(nameof(CreateSurvey))
            .WithSummary("Creates a new Survey");
    }

    private static async Task<Results<Ok<SurveyModel>, ProblemHttpResult>> GetSurvey(
        IQueryHandler<GetSurveyDetailQuery, Result<SurveyModel, Error>> handler,
        [Description("Primary key of the Survey")] int id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetSurveyDetailQuery(id), cancellationToken);

        return ApiResultExtensions.FromResult(result);
    }

    private static async Task<Results<Ok<List<UserSurveyModel>>, ProblemHttpResult>> GetUserSurveys(
        IQueryHandler<GetUserSurveysQuery, Result<List<UserSurveyModel>, Error>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetUserSurveysQuery(), cancellationToken);

        return ApiResultExtensions.FromResult(result);
    }

    private static async
        Task<Results<CreatedAtRoute<SurveyModel>, ProblemHttpResult,
            UnprocessableEntity<IDictionary<string, string[]>>>> CreateSurvey(
            ICommandHandler<CreateSurveyCommand, Result<SurveyModel, Error>> handler,
            CreateSurveyCommand command, 
            CancellationToken cancellationToken)
    {
        var result = await handler.Handle(command, cancellationToken);

        if (result.IsSuccess)
            return TypedResults.CreatedAtRoute(result.Value, nameof(GetSurvey), new { id = result.Value.Id });

        if (result.Error is ValidationError validationError)
        {
            return TypedResults.UnprocessableEntity(validationError.Errors);
        }

        return TypedResults.Problem($"Error Code: {result.Error.Code}. Error Message: {result.Error.Message}",
            statusCode: StatusCodes.Status400BadRequest);
    }
}