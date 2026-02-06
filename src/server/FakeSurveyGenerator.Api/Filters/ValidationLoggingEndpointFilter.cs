using FakeSurveyGenerator.Application.Shared.Identity;
using Microsoft.AspNetCore.Http;

namespace FakeSurveyGenerator.Api.Filters;

public sealed class ValidationLoggingEndpointFilter(ILoggerFactory loggerFactory, IUserService userService) : IEndpointFilter
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
        && value is IDictionary<string, string[]> errors
        && errors.Count > 0)
    {
      var userIdentity = userService?.GetUserIdentity() ?? "Unknown Identity";

      _logger.LogValidationErrors(endpointName, userIdentity, errors);
    }

    return result;
  }
}

public static partial class ValidationLoggingEndpointFilterLogging
{
  [LoggerMessage(
      EventId = 1,
      Level = LogLevel.Warning,
      Message = "Validation failure on Endpoint: {EndpointName} for User: {UserIdentity}. Errors: {Errors}")]
  public static partial void LogValidationErrors(
      this ILogger logger,
      string endpointName,
      string userIdentity,
      IDictionary<string, string[]> errors);
}
