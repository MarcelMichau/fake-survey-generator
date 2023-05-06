using FakeSurveyGenerator.Application.Common.Identity;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Common.Behaviours;

public sealed class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    private readonly ILogger _logger;
    private readonly IUserService _userService;

    public LoggingBehaviour(ILogger<TRequest> logger, IUserService userService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;

        _logger.LogInformation("Fake Survey Generator Request: {Name} {@Request} {@CurrentUserIdentity}",
            name, request, _userService.GetUserIdentity());

        return Task.CompletedTask;
    }
}