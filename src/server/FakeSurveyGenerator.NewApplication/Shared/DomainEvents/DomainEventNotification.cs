using FakeSurveyGenerator.Application.Domain.Shared.SeedWork;
using MediatR;

namespace FakeSurveyGenerator.Application.Shared.DomainEvents;

public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : DomainEvent;