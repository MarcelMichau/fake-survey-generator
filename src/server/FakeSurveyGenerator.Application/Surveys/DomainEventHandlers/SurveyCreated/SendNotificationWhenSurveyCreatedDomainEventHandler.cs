using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Interfaces;
using FakeSurveyGenerator.Application.Notifications.Models;
using FakeSurveyGenerator.Domain.DomainEvents;
using MediatR;

namespace FakeSurveyGenerator.Application.Surveys.DomainEventHandlers.SurveyCreated
{
    public sealed class SendNotificationWhenSurveyCreatedDomainEventHandler : INotificationHandler<SurveyCreatedDomainEvent>
    {
        private readonly INotificationService _notification;

        public SendNotificationWhenSurveyCreatedDomainEventHandler(INotificationService notification)
        {
            _notification = notification;
        }

        public async Task Handle(SurveyCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            await _notification.SendAsync(new MessageDto
            {
                Body = $"Survey with ID: {notification.Survey.Id} created",
                From = "System",
                Subject = "New Survey Created",
                To = "Whom It May Concern"
            });
        }
    }
}
