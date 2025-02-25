using MediatR;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Shared.Behaviours;

public sealed class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogUnhandledException(ex, requestName);

            throw;
        }
    }
}

public static partial class UnhandledExceptionBehaviourLogging
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Error,
        Message = "Fake Survey Generator Request: Unhandled Exception for Request {Name}")]
    public static partial void LogUnhandledException(this ILogger logger, Exception ex, string name);
}