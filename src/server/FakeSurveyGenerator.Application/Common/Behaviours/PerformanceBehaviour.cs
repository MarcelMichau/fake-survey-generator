using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Common.Behaviours;

public sealed class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;

    public PerformanceBehaviour(ILogger<TRequest> logger)
    {
        _timer = new Stopwatch();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

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