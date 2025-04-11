using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FakeSurveyGenerator.Application.Shared.Behaviours;

public sealed class PerformanceBehaviour<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();

        var response = await next(cancellationToken);

        var diff = Stopwatch.GetElapsedTime(startTime);

        if (diff.Milliseconds <= 500) return response;

        var name = typeof(TRequest).Name;

        _logger.LogLongRunningRequest(name, diff.Milliseconds);

        return response;
    }
}

public static partial class PerformanceBehaviourLogging
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Warning,
        Message = "Fake Survey Generator Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds)")]
    public static partial void LogLongRunningRequest(this ILogger logger, string name, int elapsedMilliseconds);
}