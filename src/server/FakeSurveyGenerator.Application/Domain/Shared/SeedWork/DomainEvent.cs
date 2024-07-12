namespace FakeSurveyGenerator.Application.Domain.Shared.SeedWork;

public interface IHasDomainEvents
{
    public IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void AddDomainEvent(DomainEvent eventItem);
    void RemoveDomainEvent(DomainEvent eventItem);
    void ClearDomainEvents();
}

public abstract class DomainEvent
{
    protected DomainEvent()
    {
        DateOccurred = DateTimeOffset.Now;
    }

    public DateTimeOffset DateOccurred { get; protected set; }
}