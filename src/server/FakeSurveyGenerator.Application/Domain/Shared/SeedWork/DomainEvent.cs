using MediatR;

namespace FakeSurveyGenerator.Application.Domain.Shared.SeedWork;

public interface IHasDomainEvents
{
    public IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void AddDomainEvent(DomainEvent eventItem);
    void RemoveDomainEvent(DomainEvent eventItem);
    void ClearDomainEvents();
}

public abstract class DomainEvent : INotification
{
    public DateTimeOffset DateOccurred { get; protected set; } = DateTimeOffset.Now;
}