namespace FakeSurveyGenerator.Application.DomainEvents;

public interface IDomainEventHandler
{
    Type EventType { get; }
    Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

public interface IDomainEventHandler<in TEvent> : IDomainEventHandler where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);

    Type IDomainEventHandler.EventType => typeof(TEvent);

    Task IDomainEventHandler.HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        return HandleAsync((TEvent)domainEvent, cancellationToken);
    }
}