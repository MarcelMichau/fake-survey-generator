﻿using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Shared.Errors;
using Microsoft.AspNetCore.Http.HttpResults;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace FakeSurveyGenerator.Api.Shared;

public static class ApiResultExtensions
{
    public static IResult FromResult<T>(Result<T> result)
    {
        return TypedResults.Ok(result.Value);
    }

    public static Results<Ok<T>, ProblemHttpResult> FromResult<T>(Result<T, Error> result)
    {
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : BuildProblemDetails(result,
                Equals(result.Error, Errors.General.NotFound())
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status400BadRequest);
    }

    private static ProblemHttpResult BuildProblemDetails<T>(Result<T, Error> result, int statusCode)
    {
        return TypedResults.Problem($"Error Code: {result.Error.Code}. Error Message: {result.Error.Message}",
            statusCode: statusCode);
    }
}