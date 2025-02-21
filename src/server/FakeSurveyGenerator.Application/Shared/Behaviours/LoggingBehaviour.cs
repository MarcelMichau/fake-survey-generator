using FakeSurveyGenerator.Application.Shared.Identity;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Shared.Behaviours;

public sealed class LoggingBehaviour<TRequest>(ILogger<TRequest> logger, IUserService userService)
    : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;

        _logger.LogRequest( name, request, _userService.GetUserIdentity());

        return Task.CompletedTask;
    }
}

public static partial class LoggingBehaviourLogging
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Information,
        Message = "Fake Survey Generator Request: {Name} {@Request} {@CurrentUserIdentity}")]
    public static partial void LogRequest(this ILogger logger, string name, object request, string currentUserIdentity);
}