using System;
using System.Collections.Generic;

namespace FakeSurveyGenerator.Shared.SeedWork;

public interface IHasDomainEvents
{
    public IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void AddDomainEvent(DomainEvent eventItem);
    void RemoveDomainEvent(DomainEvent eventItem);
    void ClearDomainEvents();
}

public abstract class DomainEvent
{
    public DateTimeOffset DateOccurred { get; protected set; }

    protected DomainEvent()
    {
        DateOccurred = DateTimeOffset.Now;
    }
}