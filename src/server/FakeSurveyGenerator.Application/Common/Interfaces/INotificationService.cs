using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Notifications.Models;

namespace FakeSurveyGenerator.Application.Common.Interfaces
{
    public interface INotificationService
    {
        Task SendAsync(MessageDto message);
    }
}
