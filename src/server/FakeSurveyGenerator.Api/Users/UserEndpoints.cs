using System.ComponentModel;
using FakeSurveyGenerator.Api.Shared;
using FakeSurveyGenerator.Application.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FakeSurveyGenerator.Api.Users;

internal static class UserEndpoints
{
    internal static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var userGroup = app.MapGroup("/api/user")
            .RequireAuthorization();

        userGroup.MapGet("/{id:int}", GetUser)
            .WithName(nameof(GetUser))
            .WithSummary("Retrieves a specific User")
            .WithOpenApi();

        userGroup.MapGet("isRegistered", IsRegistered)
            .WithName(nameof(IsRegistered))
            .WithSummary("Checks whether or not a User with a specific UserId is already registered in the system")
            .WithOpenApi(generatedOperation =>
            {
                var parameter = generatedOperation.Parameters[0];
                parameter.Description = "The external user identifier";
                parameter.Required = true;
                return generatedOperation;
            });

        userGroup.MapPost("register", Register)
            .WithName(nameof(Register))
            .WithSummary("Registers a new User, using the information from the access token")
            .WithOpenApi();
    }

    private static async Task<Results<Ok<UserModel>, ProblemHttpResult>> GetUser(ISender mediator,[Description("Primary key of the User")] int id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserQuery(id), cancellationToken);

        return ResultExtensions.FromResult(result);
    }

    private static async Task<IResult> IsRegistered(ISender mediator, string userId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new IsUserRegisteredQuery(userId), cancellationToken);

        return ResultExtensions.FromResult(result);
    }

    private static async Task<Results<CreatedAtRoute<UserModel>, ProblemHttpResult>> Register(ISender mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RegisterUserCommand(), cancellationToken);

        if (result.IsSuccess)
            return TypedResults.CreatedAtRoute(result.Value, nameof(GetUser), new { id = result.Value.Id });

        return TypedResults.Problem($"Error Code: {result.Error.Code}. Error Message: {result.Error.Message}",
            statusCode: StatusCodes.Status400BadRequest);
    }
}