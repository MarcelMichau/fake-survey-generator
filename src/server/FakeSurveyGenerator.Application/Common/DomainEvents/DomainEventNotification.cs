using FakeSurveyGenerator.Shared.SeedWork;
using MediatR;

namespace FakeSurveyGenerator.Application.Common.DomainEvents
{
    public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
        where TDomainEvent : DomainEvent;
}
