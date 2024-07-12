using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Shared.Behaviours;

public sealed class PerformanceBehaviour<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Stopwatch _timer = new();

    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        if (_timer.ElapsedMilliseconds <= 500) return response;

        var name = typeof(TRequest).Name;

        _logger.LogWarning(
            "Fake Survey Generator Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Request}",
            name, _timer.ElapsedMilliseconds, request);

        return response;
    }
}