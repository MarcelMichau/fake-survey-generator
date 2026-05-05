using FakeSurveyGenerator.Application.Shared.Identity;

namespace FakeSurveyGenerator.Api.Filters;

public sealed class ValidationLoggingEndpointFilter(ILoggerFactory loggerFactory, IUserService userService)
    : IEndpointFilter
{
    public const string ValidationErrorsKey = "ValidationErrors";

    private readonly ILogger _logger = loggerFactory.CreateLogger<ValidationLoggingEndpointFilter>();

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var endpointMetadataCollection = context.HttpContext.GetEndpoint()?.Metadata;
        var endpointName = endpointMetadataCollection?.GetMetadata<EndpointNameMetadata>()?.EndpointName
                           ?? "Unknown Endpoint";

        var result = await next(context);

        if (context.HttpContext.Items.TryGetValue(ValidationErrorsKey, out var value)
            && value is IDictionary<string, string[]> { Count: > 0 } errors)
        {
            if (!_logger.IsEnabled(LogLevel.Warning))
            {
                return result;
            }

            var userIdentity = userService.GetUserIdentity();
            var state = BuildLogState(endpointName, userIdentity, errors);
            var message = $"Validation failure on Endpoint: {endpointName} for User: {userIdentity}.";

            _logger.Log(LogLevel.Warning, new EventId(1, "ValidationErrors"), state, null,
                (_, _) => message);
        }

        return result;
    }

    private static IReadOnlyList<KeyValuePair<string, object?>> BuildLogState(
        string endpointName,
        string userIdentity,
        IDictionary<string, string[]> errors)
    {
        var state = new List<KeyValuePair<string, object?>>
        {
            new("EndpointName", endpointName),
            new("UserIdentity", userIdentity),
        };

        foreach (var error in errors.OrderBy(pair => pair.Key))
        {
            state.Add(new KeyValuePair<string, object?>($"Error.{error.Key}", error.Value));
        }

        return state;
    }
}
