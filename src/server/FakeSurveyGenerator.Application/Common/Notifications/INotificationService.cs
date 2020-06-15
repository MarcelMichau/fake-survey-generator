using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Notifications.Models;

namespace FakeSurveyGenerator.Application.Common.Notifications
{
    public interface INotificationService
    {
        Task SendAsync(MessageDto message);
    }
}
