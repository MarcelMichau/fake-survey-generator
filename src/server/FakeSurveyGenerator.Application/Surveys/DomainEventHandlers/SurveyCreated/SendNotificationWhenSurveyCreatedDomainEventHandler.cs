using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.DomainEvents;
using FakeSurveyGenerator.Application.Common.Notifications;
using FakeSurveyGenerator.Application.Notifications.Models;
using FakeSurveyGenerator.Domain.DomainEvents;
using MediatR;

namespace FakeSurveyGenerator.Application.Surveys.DomainEventHandlers.SurveyCreated
{
    public sealed class SendNotificationWhenSurveyCreatedDomainEventHandler : INotificationHandler<DomainEventNotification<SurveyCreatedDomainEvent>>
    {
        private readonly INotificationService _notificationService;

        public SendNotificationWhenSurveyCreatedDomainEventHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task Handle(DomainEventNotification<SurveyCreatedDomainEvent> notification, CancellationToken cancellationToken)
        {
            await _notificationService.SendMessage(new MessageDto
            {
                Body = $"Survey with ID: {notification.DomainEvent.Survey.Id} created",
                From = "System",
                Subject = "New Survey Created",
                To = "Whom It May Concern"
            }, cancellationToken);
        }
    }
}
