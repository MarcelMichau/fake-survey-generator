using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Notifications.Models;

namespace FakeSurveyGenerator.Application.Common.Notifications
{
    public interface INotificationService
    {
        Task SendMessage(MessageDto message, CancellationToken cancellationToken = default);
    }
}
