using FakeSurveyGenerator.Application.Features.Notifications;
using FakeSurveyGenerator.Application.Shared.Notifications;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Infrastructure.Notifications;

internal sealed class NotificationService(ILogger<NotificationService> logger) : INotificationService
{
    private readonly ILogger<NotificationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task SendMessage(MessageModel message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Simulating sending a notification somewhere... Message: {@message}", message);

        return Task.CompletedTask;
    }
}