using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Interfaces;
using FakeSurveyGenerator.Application.Notifications.Models;

namespace FakeSurveyGenerator.Infrastructure
{
    public class NotificationService : INotificationService
    {
        public Task SendAsync(MessageDto message)
        {
            return Task.CompletedTask;
        }
    }
}
