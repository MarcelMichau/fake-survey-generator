using System;
using System.Text.Json;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Interfaces;
using FakeSurveyGenerator.Application.Notifications.Models;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Infrastructure
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task SendAsync(MessageDto message)
        {
            _logger.LogInformation(@$"Simulating sending a notification somewhere... Message: {JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                WriteIndented = true
            })}");
            
            return Task.CompletedTask;
        }
    }
}