using FakeSurveyGenerator.Domain.SeedWork;
using MediatR;

namespace FakeSurveyGenerator.Application.Common.DomainEvents
{
    public sealed class DomainEventNotification<TDomainEvent> : INotification where TDomainEvent : DomainEvent
    {
        public TDomainEvent DomainEvent { get; }

        public DomainEventNotification(TDomainEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }
    }
}
