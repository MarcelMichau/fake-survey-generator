namespace FakeSurveyGenerator.Application.DomainEvents;

public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
}