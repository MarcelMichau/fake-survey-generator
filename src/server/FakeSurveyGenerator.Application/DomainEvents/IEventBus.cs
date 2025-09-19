namespace FakeSurveyGenerator.Application.DomainEvents;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}