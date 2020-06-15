using System;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Notifications;
using FakeSurveyGenerator.Application.Notifications.Models;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Infrastructure.Notifications
{
    internal sealed class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task SendMessage(MessageDto message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Simulating sending a notification somewhere... Message: {@message}", message);

            return Task.CompletedTask;
        }
    }
}