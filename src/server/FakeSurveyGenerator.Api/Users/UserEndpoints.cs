using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Api.Filters;
using FakeSurveyGenerator.Api.Shared;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Features.Users;
using FakeSurveyGenerator.Application.Shared.Errors;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace FakeSurveyGenerator.Api.Users;

internal static class UserEndpoints
{
    internal static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var userGroup = app.MapGroup("/api/user")
            .RequireAuthorization()
            .AddEndpointFilter<RequestLoggingEndpointFilter>();

        userGroup.MapGet("/{id:int}", GetUser)
            .WithName(nameof(GetUser))
            .WithSummary("Retrieves a specific User");

        userGroup.MapGet("isRegistered", IsRegistered)
            .WithName(nameof(IsRegistered))
            .WithSummary("Checks whether or not a User with a specific UserId is already registered in the system");

        userGroup.MapPost("register", Register)
            .WithName(nameof(Register))
            .WithSummary("Registers a new User, using the information from the access token");
    }

    private static async Task<Results<Ok<UserModel>, ProblemHttpResult>> GetUser(
        IQueryHandler<GetUserQuery, Result<UserModel, Error>> handler,
        [Description("Primary key of the User")] int id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetUserQuery(id), cancellationToken);

        return ApiResultExtensions.FromResult(result);
    }

    private static async Task<IResult> IsRegistered(
        IQueryHandler<IsUserRegisteredQuery, Result<UserRegistrationStatusModel, Error>> handler,
        [Description("The external user identifier")] [Required] string userId,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new IsUserRegisteredQuery(userId), cancellationToken);

        return ApiResultExtensions.FromResult(result);
    }

    private static async Task<Results<CreatedAtRoute<UserModel>, ProblemHttpResult>> Register(
        ICommandHandler<RegisterUserCommand, Result<UserModel, Error>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new RegisterUserCommand(), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.CreatedAtRoute(result.Value, nameof(GetUser), new { id = result.Value.Id });

        return TypedResults.Problem($"Error Code: {result.Error.Code}. Error Message: {result.Error.Message}",
            statusCode: StatusCodes.Status400BadRequest);
    }
}