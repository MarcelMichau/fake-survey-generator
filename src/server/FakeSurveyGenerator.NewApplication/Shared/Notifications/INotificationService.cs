using FakeSurveyGenerator.Application.Features.Notifications;

namespace FakeSurveyGenerator.Application.Shared.Notifications;

public interface INotificationService
{
    Task SendMessage(MessageModel message, CancellationToken cancellationToken = default);
}