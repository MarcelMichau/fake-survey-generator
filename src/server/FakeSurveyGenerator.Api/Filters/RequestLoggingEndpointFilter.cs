using FakeSurveyGenerator.Application.Shared.Identity;

namespace FakeSurveyGenerator.Api.Filters;

public sealed class RequestLoggingEndpointFilter(ILoggerFactory loggerFactory, IUserService userService) : IEndpointFilter
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<RequestLoggingEndpointFilter>();

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var endpointMetadataCollection = context.HttpContext.GetEndpoint()?.Metadata;
        var endpointName = endpointMetadataCollection?.GetMetadata<EndpointNameMetadata>()?.EndpointName ?? "Unknown Endpoint";

        var userIdentity = userService?.GetUserIdentity() ?? "Unknown Identity";

        _logger.LogRequest(endpointName, userIdentity);
        var result = await next(context);
        return result;
    }
}

public static partial class RequestLoggingEndpointFilterLogging
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Information,
        Message = "Request to Endpoint: {EndpointName} for User: {UserIdentity}")]
    public static partial void LogRequest(this ILogger logger, string endpointName, string userIdentity);
}